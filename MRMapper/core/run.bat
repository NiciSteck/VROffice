@echo off 
echo Running MRMapper's core
start "MRDocker" docker run  -it --rm ^
            -p 5000:5000 -p 5001:5001 -p 5002:5002 -p 5003:5003 -p 5004:5004 -p 5005:5005 ^
            -e DISPLAY=host.docker.internal:0.0 ^
            -v %CD%/ros_node/mr-mapper:/root/catkin_ws/src/mr-mapper ^
            mrmapper-image ^
            bash -c "dos2unix /root/catkin_ws/src/mr-mapper/src/* && source /opt/ros/noetic/setup.bash && source ~/catkin_ws/devel/setup.bash && roslaunch mr-mapper launch.xml" 

echo Running Flask Server
start "FlaskServer" python findOrientation.py