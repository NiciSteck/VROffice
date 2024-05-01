import itertools
from scipy.optimize import basinhopping,minimize
import numpy as np
from scipy.spatial.transform import Rotation
from flask import Flask, request


app = Flask(__name__)

result = {
    "quat" : [0,0,0,1],
    "error" : "0.0",
    "completed" : False,
    "envCenter" : [0,0,0],
    "mrCenter" : [0,0,0]
}

def array_permutations(labels,points):
    N = labels.size
    for permute in itertools.permutations(range(N)):
        yield labels[list(permute)],points[list(permute)], permute

def getCenteredPoints(points):
    return points - np.mean(points, axis=0)

#get rough alignment of surfaces and check get rid of noisy detections
def recursive_init_rotation(leftoverLabels, originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource, permutationArrayTarget):
    #base case
    if leftoverLabels.size == 0:
        sourceLabels = originalSourceLabels[permutationArraySource]
        sourcePoints = originalSourcePoints[permutationArraySource]
        centeredSourcePoints = sourcePoints - np.mean(sourcePoints, axis=0)

        targteLabels = originalTargetLabels[permutationArrayTarget]
        targetPoints = originalTargetPoints[permutationArrayTarget]
        centeredTargetPoints = targetPoints - np.mean(targetPoints, axis=0)

        sol = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (sourceLabels,centeredSourcePoints,list(zip(targteLabels,centeredTargetPoints)))}, niter_success=5)
        return sol.fun, sol.x, permutationArraySource, permutationArrayTarget
    
    bestRes = None
    bestRot = None
    bestPermSource = None
    bestPermTarget = None

    currLabel = leftoverLabels[0]
    indexArrayLeftover = np.where(leftoverLabels == currLabel)[0]
    indexArraySource = np.where(originalSourceLabels == currLabel)[0]
    indexArrayTarget = np.where(originalTargetLabels == currLabel)[0]
    currSourceLabels = originalSourceLabels[indexArraySource]
    currSourcePoints = originalSourcePoints[indexArraySource]
    currTargetLabels = originalTargetLabels[indexArrayTarget]
    currTargetPoints = originalTargetPoints[indexArrayTarget]

    if indexArraySource.size < indexArrayTarget.size:
        #MrMapper recognized too many planes with currLabel
        for labelsPermuted, pointsPermuted, labelPerm in array_permutations(currTargetLabels,currTargetPoints):
            cutPerm = np.array(labelPerm)[:indexArraySource.size]
            res, rot, permSource, permTarget = recursive_init_rotation(np.delete(leftoverLabels,indexArrayLeftover), originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource+indexArraySource.tolist(),permutationArrayTarget + indexArrayTarget[cutPerm].tolist())
            if bestRes is None or res  < bestRes:
                bestRes = res
                bestRot = rot
                bestPermSource = permSource
                bestPermTarget = permTarget

    elif indexArraySource.size > indexArrayTarget.size:
        #MrMapper didnt recognize all planes with currLabel
        for labelsPermuted, pointsPermuted, labelPerm in array_permutations(currSourceLabels,currSourcePoints):
            cutPerm = np.array(labelPerm)[:indexArrayTarget.size]
            res, rot, permSource, permTarget = recursive_init_rotation(np.delete(leftoverLabels,indexArrayLeftover), originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource + indexArraySource[cutPerm].tolist(),permutationArrayTarget+indexArrayTarget.tolist())
            if bestRes is None or res  < bestRes:
                bestRes = res
                bestRot = rot
                bestPermSource = permSource
                bestPermTarget = permTarget
    else:
        #MrMapper recognized the same number of planes with currLabel
        res, rot, permSource, permTarget = recursive_init_rotation(np.delete(leftoverLabels,indexArrayLeftover), originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource + indexArraySource.tolist(),permutationArrayTarget+indexArrayTarget.tolist())
        if bestRes is None or res  < bestRes:
            bestRes = res
            bestRot = rot
            bestPermSource = permSource
            bestPermTarget = permTarget

    return bestRes, bestRot, bestPermSource, bestPermTarget
        
def get_planewise_centroids(labels, points):
    centroids = np.array([np.mean(x,axis=0) for x in np.split(points, points[:,0].size/4)])
    return labels[::4], centroids

def objective(quat, envPlanesLables, envPlanesPoints, mrPlanes):
    rot = Rotation.from_quat(quat)
    fittedEnv = list(zip(envPlanesLables,Rotation.apply(rot,envPlanesPoints)))
    #find the sum of the distances between the points in fittedEnvPoints and the closest point in mrPlanesPoints if they have the same label
    sumOfDistances = 0
    for envLabel, envPoint in fittedEnv:
        minDistance = np.inf
        for mrLabel, mrPoint in mrPlanes:
            if envLabel == mrLabel:
                distance = np.linalg.norm(mrPoint-envPoint)
                if distance < minDistance:
                    minDistance = distance
        sumOfDistances += minDistance**2
    return sumOfDistances


def find_rot(envLabels, envPoints, mrLabels, mrPoints):

    originalNumberOfRecognizedPlanes = mrLabels.size

    #remove all captured surfaces which have a label that does not appear in the environment
    mrLabelsSanitized = np.copy(mrLabels)
    mrPointsSanitized = np.copy(mrPoints)
    for i in range(mrLabels.size):
        if mrLabels[i] not in envLabels:
            indexOfNoise = np.where(mrLabelsSanitized == mrLabels[i])[0]
            mrLabelsSanitized = np.delete(mrLabelsSanitized,indexOfNoise,axis=0)
            mrPointsSanitized = np.delete(mrPointsSanitized,indexOfNoise,axis=0)
    mrLabels = mrLabelsSanitized
    mrPoints = mrPointsSanitized

    #check have any surfaces left to align
    assert mrPoints.size > 0

    #check that have received points corresponding to a set of surfaces
    assert envLabels.size % 4 == 0
    assert mrLabels.size % 4 == 0

    initRot = [0,0,0,1]

    #get centroids
    envCentroidsLabels, envCentroids = get_planewise_centroids(envLabels, envPoints)
    mrCentroidsLabels, mrCentroids = get_planewise_centroids(mrLabels, mrPoints)

    #get rough rotation and the combination of surfaces that ignores noisy surfaces
    initRes, initRot, initPermEnv, initPermMr = recursive_init_rotation(envCentroidsLabels, envCentroidsLabels, envCentroids, mrCentroidsLabels, mrCentroids, [], [])

    print(initRes)

    #apply combination
    mrLabels = np.concatenate(np.array(np.split(mrLabels, mrLabels.size/4))[initPermMr])
    mrPoints = np.vstack(np.array(np.vsplit(mrPoints, mrPoints[:,0].size/4))[initPermMr])
    mrMean = np.mean(mrPoints, axis=0)
    centeredMr = mrPoints - mrMean

    envLabels = np.concatenate(np.array(np.split(envLabels, envLabels.size/4))[initPermEnv])
    envPoints = np.vstack(np.array(np.vsplit(envPoints, envPoints[:,0].size/4))[initPermEnv])
    envMean = np.mean(envPoints, axis=0)
    centeredEnv = envPoints - envMean

    #find the final rotation
    solution = basinhopping(objective, initRot, minimizer_kwargs = {"args": (envLabels,centeredEnv,list(zip(mrLabels,centeredMr)))}, niter_success=10)

    #if the alignment is bad try again
    if(solution.fun > 0.1):
        secondSolution = basinhopping(objective, solution.x, minimizer_kwargs = {"args": (envLabels,centeredEnv,list(zip(mrLabels,centeredMr)))}, niter_success=10)
        if(secondSolution.fun < solution.fun):
            print("second solution was better")
            print(secondSolution.fun)
            print("<")
            print(solution.fun)
            solution = secondSolution

    print(solution.fun)

    normalizedQuat = Rotation.from_quat(solution.x).as_quat()

    #add a penalty for ignoring surfaces through the noise elimination
    noisePenalty = (originalNumberOfRecognizedPlanes - mrLabels.size) * 0.01 #DEV COMMENT: probably sensible to adjust depending on size of environment (the bigger the env the smaller the penalty). maybe add penalty in rough alignment already (testing required)

    return normalizedQuat, solution.fun + noisePenalty , envMean, mrMean

def json_to_array(pointList):
    labels = []
    points = []
    for point in pointList:
        labels.append(point["label"])
        points.append(point["point"])
    return np.array(labels), np.array(points)

#returns the result
@app.get("/result")
def getResult():
    return result

#resets the "completed" flag of the result
@app.put("/reset")
def putReset():
    result["completed"] = False
    return result, 200

#receive input for calibration
@app.put("/result")
def putResult():
    if request.is_json:
        unityPoints = request.get_json()
        envLabels, envPoints = json_to_array(unityPoints["env"])
        mrLabels, mrPoints = json_to_array(unityPoints["mr"])
        quat, error, envMean, mrMean = find_rot(envLabels, envPoints, mrLabels, mrPoints)
        result["quat"] = quat.tolist()
        result["error"] = error
        result["completed"] = True
        result["envCenter"] = envMean.tolist()
        result["mrCenter"] = mrMean.tolist()
        return result, 200
    return {"error": "Request must be JSON"}, 415



if __name__ == "__main__":
    app.run(port=5005,debug = True)




