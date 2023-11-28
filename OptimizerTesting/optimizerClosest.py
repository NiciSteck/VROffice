from scipy.optimize import minimize
import numpy as np
import files
from scipy.spatial.transform import Rotation


def objective(flatRotMatrix,envPlanesPoints, mrPlanesPoints):
    mean_env = np.mean(envPlanesPoints, axis=0)
    mean_mr = np.mean(mrPlanesPoints, axis=0)

    centeredEnv = envPlanesPoints - mean_env
    centeredMr = mrPlanesPoints - mean_mr


    fittedEnvPoints = np.apply_along_axis(lambda x: np.matmul(flatRotMatrix.reshape(3,3),x),1,centeredEnv)
    #find the sum of the distances between the pointsin fittedEnvPoints and the closes point in mrPlanesPoints
    sumOfDistances = 0
    for point in fittedEnvPoints:
        minDistance = np.inf
        for mrPoint in centeredMr:
            distance = np.linalg.norm(point-mrPoint)
            if distance < minDistance:
                minDistance = distance
        sumOfDistances += minDistance
    return sumOfDistances

env = np.genfromtxt(files.ENV)
mr = np.genfromtxt(files.MR)


#solution = minimize(objective, np.identity(3).flatten(), args=(env,mr), method='SLSQP', options={'disp': True})
solution = minimize(objective, np.identity(3).flatten(), args=(env,mr), options={'disp': True})
R = solution.x.reshape(3,3)
print(R)
r =  Rotation.from_matrix(R)
angles = r.as_euler("xyz",degrees=True)
print(angles)
