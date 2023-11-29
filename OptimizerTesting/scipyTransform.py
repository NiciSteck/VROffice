import numpy as np
import files
from scipy.spatial.transform import Rotation

env = np.genfromtxt(files.ENV)
mr = np.genfromtxt(files.MR)

mean_env = np.mean(env, axis=0)
mean_mr = np.mean(mr, axis=0)

centeredEnv = env - mean_env
centeredMr = mr - mean_mr

rot,res = Rotation.align_vectors(centeredEnv,centeredMr)
print(res)
sol = rot.as_quat()
print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))
