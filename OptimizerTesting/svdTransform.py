import numpy as np
import files
from scipy.spatial.transform import Rotation
import matplotlib.pyplot as plt

# Input: expects 3xN matrix of points
# Returns R,t
# R = 3x3 rotation matrix
# t = 3x1 column vector

def rigid_transform_3D(A, B):
    assert A.shape == B.shape

    num_rows, num_cols = A.shape
    if num_rows != 3:
        raise Exception(f"matrix A is not 3xN, it is {num_rows}x{num_cols}")

    num_rows, num_cols = B.shape
    if num_rows != 3:
        raise Exception(f"matrix B is not 3xN, it is {num_rows}x{num_cols}")

    # find mean column wise
    centroid_A = np.mean(A, axis=1)
    centroid_B = np.mean(B, axis=1)

    # ensure centroids are 3x1
    centroid_A = centroid_A.reshape(-1, 1)
    centroid_B = centroid_B.reshape(-1, 1)

    # subtract mean
    Am = A - centroid_A
    Bm = B - centroid_B

    H = Am @ np.transpose(Bm)

    # sanity check
    #if linalg.matrix_rank(H) < 3:
    #    raise ValueError("rank of H = {}, expecting 3".format(linalg.matrix_rank(H)))

    # find rotation
    U, S, Vt = np.linalg.svd(H)
    R = Vt.T @ U.T

    # special reflection case
    if np.linalg.det(R) < 0:
        print("det(R) < R, reflection detected!, correcting for it ...")
        Vt[2,:] *= -1
        R = Vt.T @ U.T

    t = -R @ centroid_A + centroid_B

    return R, t

env = np.genfromtxt(files.ENV)
mr = np.genfromtxt(files.MR)

solution = rigid_transform_3D(env.T,mr.T)
rotation = solution[0]
r =  Rotation.from_matrix(rotation)
sol = r.as_quat()
print("({}f, {}f, {}f, {}f)".format(sol[0],sol[1],sol[2],sol[3]))
angles = r.as_euler("zyx",degrees=True)
print(angles)

from_x = r.apply(env)[:, 0] + solution[1][0]
from_y = r.apply(env)[:, 1] + solution[1][1]
plt.figure(figsize=(6,6))
jitter = 0.005 # make points visible
plt.scatter(from_x + jitter, from_y + jitter)
plt.scatter(mr[:, 0], mr[:, 1])
plt.show()