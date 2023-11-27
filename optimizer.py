from scipy.optimize import minimize
import numpy as np

#what if we just use SVD variant?
def objective(flatRotMatrix,envPlanesPoints, mrPlanesPoints):
    mean_env = np.mean(envPlanesPoints, axis=0)
    mean_mr = np.mean(mrPlanesPoints, axis=0)

    centeredEnv = envPlanesPoints - mean_env
    centeredMr = mrPlanesPoints - mean_mr

    fittedEnvPoints = np.dot(centeredEnv,flatRotMatrix.reshape(3,3))
    #find the sum of the distances between the pointsin fittedEnvPoints and the closes point in mrPlanesPoints
    sumOfDistances = 0 
    for point in centeredEnv:
        minDistance = np.inf
        for mrPoint in centeredMr:
            distance = np.linalg.norm(point-mrPoint)
            if distance < minDistance:
                minDistance = distance
        sumOfDistances += minDistance
    return sumOfDistances

env = np.genfromtxt("envPlaneFile.txt")
mr = np.genfromtxt("mrPlaneFile.txt")


solution = minimize(objective, np.identity(3).flatten(), args=(env,mr), method='SLSQP', options={'disp': True})
#solution = minimize(objective, np.identity(3).flatten(), args=(X_mov,X_fix), options={'disp': True})
print(solution.x.reshape(3,3))

