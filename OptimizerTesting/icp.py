from simpleicp import PointCloud, SimpleICP
import numpy as np
import files
import matplotlib.pyplot as plt

env = np.genfromtxt(files.ENV)
mr = np.genfromtxt(files.MR)

mean_env = np.mean(env, axis=0)
mean_mr = np.mean(mr, axis=0)

centeredEnv = env - mean_env
centeredMr = mr - mean_mr

pc_fix = PointCloud(centeredMr, columns=["x", "y", "z"])
pc_mov = PointCloud(centeredEnv, columns=["x", "y", "z"])

icp = SimpleICP()
icp.add_point_clouds(pc_fix, pc_mov)

H, X_mov_transformed, rigid_body_transformation_params, distance_residuals = icp.run(neighbors=10)


plt.figure(figsize=(6,6))
jitter = 0.005 # make points visible
plt.scatter(X_mov_transformed[:,0] + jitter, X_mov_transformed[:,1] + jitter)
plt.scatter(centeredMr[:, 0], centeredMr[:, 1])
plt.show()