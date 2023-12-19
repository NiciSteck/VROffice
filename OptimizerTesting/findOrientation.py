import math
import itertools
from tqdm import tqdm
from scipy.optimize import basinhopping,minimize
import numpy as np
import files
from scipy.spatial.transform import Rotation
import matplotlib.pyplot as plt
from flask import Flask, render_template, request


app = Flask(__name__)

result = {
    "quat" : [0,0,0,1],
    "error" : "0.0",
    "completed" : False
}

def array_permutations(labels,points):
    N = labels.size
    for permute in itertools.permutations(range(N)):
        yield labels[list(permute)],points[list(permute)], permute

def find_init_rotation_old(sourceLabels, sourcePoints, targteLabels, targetPoints):
    bestRes = None
    bestRot = None
    bestPerm = None

    debugCount = 0

    for labelsPermuted,pointsPermuted, perm in tqdm(array_permutations(sourceLabels), total=math.factorial(sourceLabels.size)):     
        if np.array_equal(targteLabels, labelsPermuted):
            rot, res = Rotation.align_vectors(pointsPermuted, targetPoints)

            aplliedPoints = rot.apply(pointsPermuted)
            fig = plt.figure(figsize=(6,6))
            ax = fig.add_subplot(projection='3d')
            jitter = 0.005 # make points visible
            ax.scatter(aplliedPoints[:,0] + jitter, aplliedPoints[:,1] + jitter, aplliedPoints[:,2] + jitter)
            ax.scatter(targetPoints[:, 0], targetPoints[:, 1], targetPoints[:, 2])
            plt.savefig("init%s.png"%(debugCount))

            if bestRes is None or res < bestRes:
                bestRes = res
                bestRot = rot
                bestPerm = perm

            debugCount += 1

    return bestRot.as_quat, bestRes, bestPerm    

def find_init_rotation(sourceLabels, sourcePoints, targteLabels, targetPoints):
    #only works if the missrecognized planes are >= or <= the numer of actual planes for ALL LABELS. Not good enough
    bestRes = None
    bestRot = None
    bestPerm = None

    if sourceLabels.size < targteLabels.size:
        #MrMapper recognized too many planes
        for labelsPermuted,pointsPermuted, perm in tqdm(array_permutations(targteLabels,targetPoints), total=math.factorial(targteLabels.size)):
            if np.array_equal(sourceLabels, labelsPermuted[:,:sourceLabels.size]):
                sol = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (sourceLabels,sourcePoints,list(zip(labelsPermuted[:,:sourceLabels.size],pointsPermuted[:,:sourceLabels.size])))}, disp=True, niter_success=5)
                if bestRes is None or sol.fun < bestRes:
                    bestRes = sol.fun
                    bestRot = sol.x
                    bestPerm = perm

    elif sourceLabels.size > targteLabels.size:
        #MrMapper didnt recognize all planes
        for labelsPermuted,pointsPermuted, perm in tqdm(array_permutations(sourceLabels), total=math.factorial(sourceLabels.size)):
            if np.array_equal(targteLabels, labelsPermuted[:,:targteLabels.size]):
                sol = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (labelsPermuted[:,:targteLabels.size],pointsPermuted[:,:targteLabels.size],list(zip(targteLabels,targetPoints)))}, disp=True, niter_success=5)
                if bestRes is None or sol.fun < bestRes:
                    bestRes = sol.fun
                    bestRot = sol.x
                    bestPerm = perm
    else:       
        #MrMapper recognized the same number of planes
        sol = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (sourceLabels,sourcePoints,list(zip(targteLabels,targetPoints)))}, disp=True, niter_success=5)


    return bestRes, bestRot, bestPerm

def recursive_init_rotation(leftoverLabels, originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource, permutationArrayTarget):
    #base case
    if leftoverLabels.size == 0:
        print(permutationArraySource)
        print(permutationArrayTarget)
        sourceLabels = originalSourceLabels[permutationArraySource]
        sourcePoints = originalSourcePoints[permutationArraySource]
        targteLabels = originalTargetLabels[permutationArrayTarget]
        targetPoints = originalTargetPoints[permutationArrayTarget]
        sol = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (sourceLabels,sourcePoints,list(zip(targteLabels,targetPoints)))}, disp=True, niter_success=5)
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
            res, rot, permSource, permTarget = recursive_init_rotation(np.delete(leftoverLabels,indexArrayLeftover), originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource+indexArraySource.tolist(),permutationArrayTarget + indexArrayTarget[labelPerm[:indexArraySource.size]].tolist())
            if bestRes is None or res  < bestRes:
                bestRes = res
                bestRot = rot
                bestPermSource = permSource
                bestPermTarget = permTarget

    elif indexArraySource.size > indexArrayTarget.size:
        #MrMapper didnt recognize all planes with currLabel
        for labelsPermuted, pointsPermuted, labelPerm in array_permutations(currSourceLabels,currSourcePoints):
            res, rot, permSource, permTarget = recursive_init_rotation(np.delete(leftoverLabels,indexArrayLeftover), originalSourceLabels, originalSourcePoints, originalTargetLabels, originalTargetPoints, permutationArraySource + indexArraySource[labelPerm[:indexArrayTarget.size]].tolist(),permutationArrayTarget+indexArrayTarget.tolist())
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
    #find the sum of the distances between the pointsin fittedEnvPoints and the closes point in mrPlanesPoints
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
    # envLabels = np.genfromtxt(files.ENV,dtype=str)[:,0]
    # print(envLabels)
    # envPoints = np.genfromtxt(files.ENV)[:,1:]
    # print(envPoints)

    # mrLabels = np.genfromtxt(files.MR,dtype=str)[:,0]
    # mrPoints = np.genfromtxt(files.MR)[:,1:]

    #sanitize MrPlanes
    mrLabelsSanitized = mrLabels
    mrPointsSanitized = mrPoints
    for i in range(mrLabels.size):
        if mrLabels[i] not in envLabels:
            mrLabelsSanitized = np.delete(mrLabelsSanitized,i,axis=0)
            mrPointsSanitized = np.delete(mrPointsSanitized,i,axis=0)
    mrLabels = mrLabelsSanitized
    mrPoints = mrPointsSanitized

    #check that we received all the planes
    assert envLabels.size % 4 == 0
    assert mrLabels.size % 4 == 0

    mean_env = np.mean(envPoints, axis=0)
    mean_mr = np.mean(mrPoints, axis=0)

    centeredEnv = envPoints - mean_env
    centeredMr = mrPoints - mean_mr

    initRot = [0,0,0,1]


    envCentroidsLabels, envCentroids = get_planewise_centroids(envLabels, centeredEnv)
    mrCentroidsLabels, mrCentroids = get_planewise_centroids(mrLabels, centeredMr)

    initRes, initRot, initPermEnv, initPermMr = recursive_init_rotation(envCentroidsLabels, envCentroidsLabels, envCentroids, mrCentroidsLabels, mrCentroids, [], [])
    print(initRes)

    centeredMr = np.vstack(np.array(np.vsplit(centeredMr, centeredMr[:,0].size/4))[initPermMr])
    centeredEnv = np.vstack(np.array(np.vsplit(centeredEnv, centeredEnv[:,0].size/4))[initPermEnv])

    solution = basinhopping(objective, initRot, minimizer_kwargs = {"args": (envLabels,centeredEnv,list(zip(mrLabels,centeredMr)))}, disp=True, niter_success=10)
    # solution = minimize(objective, initRot, args=(envLabels,centeredEnv,list(zip(mrLabels,centeredMr))), options={'disp': True})

    sol = solution.x
    print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))
    r = Rotation.from_quat(sol)

    from_x = r.apply(centeredEnv)[:, 0]
    from_y = r.apply(centeredEnv)[:, 1]
    plt.figure(figsize=(12,12))
    jitter = 0.005 # make points visible
    plt.scatter(from_x + jitter, from_y + jitter)
    plt.scatter(centeredMr[:, 0], centeredMr[:, 1])
    plt.savefig("final2d.png")

    print(solution.fun)
    print(solution.x)
    normalizedQuat = r.as_quat()
    print(normalizedQuat)

    return normalizedQuat, solution.fun 

def json_to_array(pointList):
    labels = []
    points = []
    for point in pointList:
        labels.append(point["label"])
        points.append(point["point"])
    return np.array(labels), np.array(points)

@app.get("/result")
def getResult():
    return result

@app.put("/reset")
def putReset():
    result["completed"] = False
    return result, 200

@app.put("/result")
def putResult():
    if request.is_json:
        unityPoints = request.get_json()
        envLabels, envPoints = json_to_array(unityPoints["env"])
        mrLabels, mrPoints = json_to_array(unityPoints["mr"])
        quat, error = find_rot(envLabels, envPoints, mrLabels, mrPoints)
        result["quat"] = quat.tolist()
        result["error"] = error
        result["completed"] = True
        return result, 200
    return {"error": "Request must be JSON"}, 415



if __name__ == "__main__":
    app.run(port=5005,debug = True)




