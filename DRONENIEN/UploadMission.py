import math
from datetime import time
import time
import airsim

import numpy as np
import os
import tempfile
import pprint
import cv2
#---------------------
import asyncio

from mavsdk import System
from mavsdk.mission import (MissionItem, MissionPlan)
import json

pathjson = 'D:/CODE/DRONE/WAYPOINTS/A5.plan'
pathwaypoints = 'D:/CODE/DRONE/WAYPOINTS/A5.waypoints'
pathresults = 'D:/CODE/DRONE/WAYPOINTS/B5.waypoints'
pathresultsjson = 'D:/CODE/DRONE/WAYPOINTS/B5.plan'

async def load_waypoints_from_plan(plan_file):
    with open(plan_file, 'r') as file:
        plan_data = json.load(file)

    waypoints = plan_data.get('mission').get('items')['param']
    return waypoints


async def execute_mission(client, waypoints):
    for waypoint in waypoints:
        lat, lon, alt = waypoint['latitude'], waypoint['longitude'], waypoint['altitude']

        # Move the drone to the waypoint
        await client.moveToPositionAsync(lat, lon, alt, velocity=5.0)


async def takeoff_and_run_mission(client, waypointsfile, target_altitude=50.0):
    # Arm the drone
    # Take off to the specified altitude
    client.takeoffAsync(timeout_sec=60, vehicle_name="")
    client.moveToPositionAsync(0, 0, target_altitude, velocity=5.0)

    # waypoints = load_waypoints_from_plan(waypointsfile)
    # Execute the mission
    # execute_mission(client, waypoints)
    #
    # # Land the drone
    # await client.landAsync(timeout_sec=60, vehicle_name="")
    #
    # # Disarm the drone
    # await client.armDisarm(False)

client = airsim.MultirotorClient()
client.confirmConnection()
client.enableApiControl(True)
# Run the asynchronous code
asyncio.run(takeoff_and_run_mission(client, pathresultsjson))

