import math
import itertools
from tqdm import tqdm
from scipy.optimize import basinhopping
import numpy as np
import files
from scipy.spatial.transform import Rotation
import matplotlib.pyplot as plt


def array_permutations(array):
    N = array.size
    for permute in itertools.permutations(range(N)):
        yield array[list(permute)], permute

def find_init_rotation(sourceLabels, sourcePoints, targteLabels, targetPoints):
    bestRes = None
    bestRot = None
    bestPerm = None
    for labelsPermuted, perm in tqdm(array_permutations(sourceLabels), total=math.factorial(sourceLabels.size)):     
        if np.array_equal(targteLabels, labelsPermuted):
            rot, res = Rotation.align_vectors(sourcePoints[list(perm)], targetPoints)
            if bestRes is None or res < bestRes:
                bestRes = res
                bestRot = rot
                bestPerm = perm
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


def main():
    envLabels = np.genfromtxt(files.ENV,dtype=str)[:,0]
    print(envLabels)
    envPoints = np.genfromtxt(files.ENV)[:,1:]
    print(envPoints)

    mrLabels = np.genfromtxt(files.MR,dtype=str)[:,0]
    mrPoints = np.genfromtxt(files.MR)[:,1:]


    #check that we received all the planes
    assert envLabels.size % 4 == 0
    assert mrLabels.size % 4 == 0
    assert envLabels.size == mrLabels.size

    finalRotation = Rotation.from_quat([0,0,0,1])

    mean_env = np.mean(envPoints, axis=0)
    mean_mr = np.mean(mrPoints, axis=0)

    centeredEnv = envPoints - mean_env
    centeredMr = mrPoints - mean_mr
    centeredEnvInit = centeredEnv

    if(envLabels.size/4 >2):
        envCentroidsLabels, envCentroids = get_planewise_centroids(envLabels, centeredEnv)

        mrCentroidsLabels, mrCentroids = get_planewise_centroids(mrLabels, centeredMr)

        initRot, initRes, initPerm = find_init_rotation(envCentroidsLabels, envCentroids, mrCentroidsLabels, mrCentroids)

        print(initRes)
        from_x = initRot.apply(envCentroids)[:, 0]
        from_y = initRot.apply(envCentroids)[:, 1]
        plt.figure(figsize=(6,6))
        jitter = 0.005 # make points visible
        plt.scatter(from_x + jitter, from_y + jitter)
        plt.scatter(mrCentroids[:, 0], mrCentroids[:, 1])
        plt.show()

        centeredEnvInit = initRot.apply(centeredEnv)
        finalRotation *= initRot

    solution = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (envLabels,centeredEnvInit,list(zip(mrLabels,centeredMr)))}, disp=True)

    sol = solution.x
    print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))


    finalRotation *= Rotation.from_quat(sol)
    angles = finalRotation.as_euler("xyz",degrees=True)
    print(angles)
    from_x = finalRotation.apply(centeredEnv)[:, 0]
    from_y = finalRotation.apply(centeredEnv)[:, 1]
    plt.figure(figsize=(6,6))
    jitter = 0.005 # make points visible
    plt.scatter(from_x + jitter, from_y + jitter)
    plt.scatter(centeredMr[:, 0], centeredMr[:, 1])
    plt.show()

if __name__ == "__main__":
    main()




