from scipy.optimize import basinhopping
import numpy as np
import files
from scipy.spatial.transform import Rotation
import matplotlib.pyplot as plt


envPoints = np.genfromtxt(files.ENV)[:,1:]
print(envPoints)
envLabels = np.genfromtxt(files.ENV,dtype=str)[:,0]
print(envLabels)
mrPoints = np.genfromtxt(files.MR)[:,1:]
mrLabels = np.genfromtxt(files.MR,dtype=str)[:,0]
#check that we received all the planes
assert envPoints[:,0].size % 4 == 0
assert mrPoints[:,0].size % 4 == 0
assert envPoints[:,0].size == mrPoints[:,0].size

mean_env = np.mean(envPoints, axis=0)
mean_mr = np.mean(mrPoints, axis=0)

centeredEnv = envPoints - mean_env
centeredMr = mrPoints - mean_mr

#calculate the centroid of each plane in the input
envCentroids = np.array([np.mean(x,axis=0) for x in np.split(centeredEnv, centeredEnv[:,0].size/4)])
envCentroidsLabels = envLabels[::4]
print(envCentroids)
print(envCentroidsLabels)
mrCentroids = np.array([np.mean(x,axis=0) for x in np.split(centeredMr, centeredMr[:,0].size/4)])
mrCentroidsLabels = mrLabels[::4]


