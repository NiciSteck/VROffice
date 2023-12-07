#!/usr/bin/env python3
import socket as s 
import numpy as np


indexLog = 0
f = open('PyOdomLog.txt','w')

with s.socket(s.AF_INET, s.SOCK_STREAM) as sock:
    sock.connect((s.gethostname(), ---))
    while True:
        lenb = sock.recv(4)
        len = int.from_bytes(lenb, byteorder='big', signed=False)
        data = sock.recv(len)
        arr = np.frombuffer(data, dtype=np.float32)
        txt = "{}: ({:.5f}, {:.5f}, {:.5f}), ({:.5f}, {:.5f}, {:.5f}, {:.5f})".format(indexLog, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6])
        f.write(txt)
        f.write("\n")
        indexLog += 1
