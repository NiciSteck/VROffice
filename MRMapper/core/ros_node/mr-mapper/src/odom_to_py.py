#!/usr/bin/env python3
import rospy
import numpy as np
from geometry_msgs.msg import PoseWithCovarianceStamped

from unity_sender import UnitySender 
import constants

""" 
This node sends the current camera position and orientation from rtabmap_ros to unity. 
"""
def callback_odompy(odom):
    global indexLog
    position = odom.pose.pose.position 
    orientation = odom.pose.pose.orientation 
    data = np.array([position.x, position.y, position.z, orientation.x, orientation.y, orientation.z, orientation.w], dtype=np.float32).tobytes()
    sender_odom.send(data)
    

def main():    
    rospy.init_node('odom_to_py', anonymous=True)
    # create UnitySender for odometry data
    global sender_odom
    sender_odom = UnitySender(constants.HOST, 5005, 'OdomPy Sender')
    sender_odom.start()

    rospy.Subscriber("/rtabmap/localization_pose", PoseWithCovarianceStamped, callback_odompy)
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




