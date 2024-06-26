from scipy.optimize import basinhopping
import numpy as np
import files
from scipy.spatial.transform import Rotation
import matplotlib.pyplot as plt


def objective(quat,envPlanesPoints, mrPlanesPoints):
    rot = Rotation.from_quat(quat)
    
    fittedEnvPoints = Rotation.apply(rot,envPlanesPoints)
    #find the sum of the distances between the pointsin fittedEnvPoints and the closes point in mrPlanesPoints
    sumOfDistances = 0
    for point in fittedEnvPoints:
        minDistance = np.inf
        for mrPoint in mrPlanesPoints:
            distance = np.linalg.norm(mrPoint-point)
            if distance < minDistance:
                minDistance = distance
        sumOfDistances += minDistance**2
    return sumOfDistances

env = np.genfromtxt(files.ENV)
mr = np.genfromtxt(files.MR)

mean_env = np.mean(env, axis=0)
mean_mr = np.mean(mr, axis=0)

centeredEnv = env - mean_env
centeredMr = mr - mean_mr

solution = basinhopping(objective, [0,0,0,1], minimizer_kwargs = {"args": (centeredEnv,centeredMr)}, disp=True)
sol = solution.x
print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))
r = Rotation.from_quat(sol)
angles = r.as_euler("xyz",degrees=True)
print(angles)


from_x = r.apply(centeredEnv)[:, 0]
from_y = r.apply(centeredEnv)[:, 1]
plt.figure(figsize=(6,6))
jitter = 0.005 # make points visible
plt.scatter(from_x + jitter, from_y + jitter)
plt.scatter(centeredMr[:, 0], centeredMr[:, 1])
plt.show()