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

def array_permutations(array):
    N = array.size
    for permute in itertools.permutations(range(N)):
        yield array[list(permute)], permute

def find_init_rotation(sourceLabels, sourcePoints, targteLabels, targetPoints):
    bestRes = None
    bestRot = None
    bestPerm = None

    debugCount = 0

    for labelsPermuted, perm in tqdm(array_permutations(sourceLabels), total=math.factorial(sourceLabels.size)):     
        if np.array_equal(targteLabels, labelsPermuted):
            rot, res = Rotation.align_vectors(sourcePoints[list(perm)], targetPoints)

            aplliedPoints = rot.apply(sourcePoints[list(perm)])
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

    return bestRot, bestRes, bestPerm        

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

    #check that we received all the planes
    assert envLabels.size % 4 == 0
    assert mrLabels.size % 4 == 0
    assert envLabels.size == mrLabels.size

    finalRotation = Rotation.from_quat([0,0,0,1])

    mean_env = np.mean(envPoints, axis=0)
    mean_mr = np.mean(mrPoints, axis=0)
    print(mean_env)
    print(mean_mr)

    centeredEnv = envPoints - mean_env
    centeredMr = mrPoints - mean_mr

    initRot = [0,0,0,1]
    if(envLabels.size/4 >2):
        envCentroidsLabels, envCentroids = get_planewise_centroids(envLabels, centeredEnv)

        mrCentroidsLabels, mrCentroids = get_planewise_centroids(mrLabels, centeredMr)

        scipyRot, initRes, initPerm = find_init_rotation(envCentroidsLabels, envCentroids, mrCentroidsLabels, mrCentroids)

        print(initRes)

        initRot = scipyRot.as_quat()


    solution = basinhopping(objective, initRot, minimizer_kwargs = {"args": (envLabels,centeredEnv,list(zip(mrLabels,centeredMr)))}, disp=True, niter_success=10)
    # solution = minimize(objective, initRot, args=(envLabels,centeredEnv,list(zip(mrLabels,centeredMr))), options={'disp': True})

    sol = solution.x
    print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))
    r = Rotation.from_quat(sol)

    rom_x = r.apply(centeredEnv)[:, 0]
    rom_y = r.apply(centeredEnv)[:, 1]
    rom_z = r.apply(centeredEnv)[:, 2]
    fig = plt.figure(figsize=(12,12))

    ax = fig.add_subplot(projection='3d')
    jitter = 0.005 # make points visible
    plt.scatter(rom_x + jitter, rom_y + jitter, rom_z + jitter)
    plt.scatter(centeredMr[:, 0], centeredMr[:, 1], centeredMr[:, 2])
    plt.savefig("final.png")

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




