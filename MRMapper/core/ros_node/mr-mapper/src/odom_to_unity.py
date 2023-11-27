#!/usr/bin/env python3
import rospy
import numpy as np
from geometry_msgs.msg import PoseWithCovarianceStamped
from nav_msgs.msg import Odometry

from unity_sender import UnitySender 
import constants

""" 
This node sends the current camera position and orientation from rtabmap_ros to unity. 
"""
indexLog = 0
f = open('ROSOdomLog.txt','w')

def callback_odom(odom):
    global indexLog
    position = odom.pose.pose.position 
    orientation = odom.pose.pose.orientation 
    arr = np.array([position.x, position.y, position.z, orientation.x, orientation.y, orientation.z, orientation.w], dtype=np.float32)
    data = arr.tobytes()
    txt = "{}: ({:.5f}, {:.5f}, {:.5f}), ({:.5f}, {:.5f}, {:.5f}, {:.5f})".format(indexLog, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6])
    f.write(txt)
    f.write('\n')
    indexLog = indexLog + 1

    sender_odom.send(data)
    

def main():    
    rospy.init_node('odom_to_unity', anonymous=True)
    # create UnitySender for odometry data
    global sender_odom
    sender_odom = UnitySender(constants.HOST, constants.PORT_ODOM, 'Odom Sender')
    sender_odom.start()

    rospy.Subscriber("/rtabmap/localization_pose", PoseWithCovarianceStamped, callback_odom)
    rospy.on_shutdown(shutdown)
    rospy.spin()


def shutdown():
    sender_odom.stop()
    print("sense_making shutdown")


if __name__ == '__main__':
    try:
        main()
    finally:
        shutdown()




