import numpy as np
import files
from scipy.spatial.transform import Rotation
import matplotlib.pyplot as plt

env = np.genfromtxt(files.ENV)
mr = np.genfromtxt(files.MR)

mean_env = np.mean(env, axis=0)
mean_mr = np.mean(mr, axis=0)

centeredEnv = env - mean_env
centeredMr = mr - mean_mr

r,res = Rotation.align_vectors(centeredEnv,centeredMr)
print(res)
sol = r.as_quat()
print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))

from_x = r.apply(centeredEnv)[:, 0]
from_y = r.apply(centeredEnv)[:, 1]
plt.figure(figsize=(6,6))
jitter = 0.005 # make points visible
plt.scatter(from_x + jitter, from_y + jitter)
plt.scatter(centeredMr[:, 0], centeredMr[:, 1])
plt.show()