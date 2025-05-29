import setup_path
import airsim
import json
import numpy as np
import os
import tempfile
import pprint
import cv2
import asyncio
pathjson = 'D:/CODE/DRONE/WAYPOINTS/A5.plan'
pathwaypoints = 'D:/CODE/DRONE/WAYPOINTS/A5.waypoints'
pathresults = 'D:/CODE/DRONE/WAYPOINTS/B5.waypoints'
pathresultsjson = 'D:/CODE/DRONE/WAYPOINTS/B2.plan'

def playmission(path, z):
    print("flying on path...")
    result = client.moveOnPathAsync([airsim.Vector3r(125, 0, z),
                                     airsim.Vector3r(125, -130, z),
                                     airsim.Vector3r(0, -130, z),
                                     airsim.Vector3r(0, 0, z)],
                                    12, 120,
                                    airsim.DrivetrainType.ForwardOnly, airsim.YawMode(False, 0), 20, 1).join()


    client.moveToPositionAsync(0, 0, z, 1).join()
    print("landing...")
    client.landAsync().join()
    print("disarming...")
    client.armDisarm(False)
    client.enableApiControl(False)
    print("done.")

def readmission(filename):
    with open(filename, 'r') as file:
        waypoints = json.load(file)
    return waypoints.get('mission').get('items')




async def runmission(z):
    print("Taking off...")
    client.armDisarm(True)
    client.takeoffAsync().join()

    waypoints = readmission(pathresultsjson)
    for waypoint in waypoints:
        x = -waypoint['params'][4]
        y = -waypoint['params'][5]
        z = -waypoint['params'][6]
        print('Go to ', x, y, x)
    # airsim.wait_key('Press any key to move vehicle to (-10, 10, -10) at 5 m/s')
    #     await client.moveToGPSAsync(latitude=x, longitude=y, altitude=z, velocity=5).join()
        client.moveToPositionAsync(x=x, y=y, z=z, velocity=5).join()


    client.goHomeAsync()

    # that's enough fun for now. let's quit cleanly
    client.enableApiControl(False)

#
# connect to the AirSim simulator
client = airsim.MultirotorClient()
client.confirmConnection()
client.enableApiControl(True)
asyncio.run(runmission(50))

