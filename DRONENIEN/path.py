import setup_path
import airsim
import sys
import time
import json
from MILP import MILP0
from CPP_data import *
from datetime import *
from mission import *
import matplotlib.pyplot as plt

pathzone = 'C:/Users/vannha2004/Documents/QGroundControl/Missions/A1.plan'
pathzonewaypoints = 'C:/Users/vannha2004/Documents/QGroundControl/Missions/A1.waypoints'

pathmission = 'C:/Users/vannha2004/Documents/QGroundControl/Missions/B1.waypoints'
pathmissionjson = 'C:/Users/vannha2004/Documents/QGroundControl/Missions/B1.plan'

def MILP(zonefile, droneAltitute = 50, droneSpeed = 15):

    zone = loadplan(zonefile)
    print('Map decomposition ...................', end='')
    Area, points = GenMissionArea(zone)
    points = checkPoints(Area, points)
    print('Done !')

    x = []
    y = []
    for i in range(len(Area)):
        x.append(Area[i].x)
        y.append(Area[i].y)

    plt.subplot(1,1,1)
    plt.scatter(y, x)

    obstacles = [[9, 6], [9, 7], [8, 7], [7, 7], [9, 8], [8, 8], [7, 8], [9, 9], [8, 9], [7, 9], [6, 9], [9, 10],
                  [8, 10], [7, 10], [6, 10], [5, 10], [9, 11], [8, 11], [7, 11], [6, 11], [5, 11], [4, 11]]

    print('Generate data .......................', end='')
    NEI, ANG = getIndex(points)
    DM = distancematrix(points, ANG, len(ANG) - 1)
    DFD = adddistancefromdepot(points, Area[0], len(NEI))
    DM = addcolumn(DM, DFD)
    NEI, ANG = addDepot(NEI, ANG, 1)

    DATA = CPP_data(len(NEI) - 1, ND=1)
    DATA.NEI = NEI
    DATA.ANG = ANG
    DATA.E = DM
    eu = [1000000, 1000000]
    DATA.set_eu(eu)


    DATA.generateR()
    print('Done !')
    print('Build MILP model ....................', end='')
    model = MILP0(DATA)
    #model.print_lp_string()
    print('Done !')
    start = datetime.now()
    print('Solve MILP model ....................', end='')
    model.solve()
    end = datetime.now()
    print('Time: ', end - start)
    #model.print_solution()
    # way = model.extractsolution(pathmission, Area, points,50, len(NEI)-1)
    way = model.getsolution(Area, points, len(NEI) - 1)
    model.exportToJson(pathmissionjson, way, att=droneAltitute, speed=droneSpeed)
    return way


def convertplan(plan, home):
    misson = []
    for i in range(len(plan)):
        t =[(plan[i][0]-home[0])*MeterOfLat, (plan[i][1]-home[1])*MeterOfLon, home[2]-plan[i][2]]
        misson.append(t)
    return misson

def converttopath(mission, z):
    home = [mission[0][4], mission[0][5], 0]
    plan = []
    for waypoint in mission:
        plan.append([waypoint[4], waypoint[5], z])
    distance = convertplan(plan, home)
    mission = []
    for x in distance:
        point = airsim.Vector3r(x_val=x[0], y_val=x[1], z_val=x[2])
        mission.append(point)
    return mission

def loadplan(plan_file):
    with open(plan_file, 'r') as file:
        plan_data = json.load(file)
    waypoints = plan_data.get('mission').get('items')
    home = plan_data.get('mission').get('plannedHomePosition')
    plan = []
    for waypoint in waypoints:
        if waypoint['params'][4] != 0:
            plan.append(waypoint['params'])
    return plan


def loadsolution(plan_solution_file):
    with open(plan_solution_file, 'r') as file:
        plan_data = json.load(file)
    waypoints = plan_data.get('mission').get('items')
    home = plan_data.get('mission').get('plannedHomePosition')
    print('home ', home)
    plan = []
    for waypoint in waypoints:
        plan.append([waypoint['params'][4], waypoint['params'][5], waypoint['params'][6]])
    distance = convertplan(plan, home)
    mission = []
    for x in distance:
        point = airsim.Vector3r(x_val=x[0], y_val=x[1], z_val=x[2])
        mission.append(point)

    return mission

def takeoff(client):
    print("arming the drone...")
    client.armDisarm(True)
    state = client.getMultirotorState()
    if state.landed_state == airsim.LandedState.Landed:
        print("taking off...")
        client.takeoffAsync().join()
    else:
        client.hoverAsync().join()
    time.sleep(1)
    state = client.getMultirotorState()
    if state.landed_state == airsim.LandedState.Landed:
        print("take off failed...")
        sys.exit(1)

def takeofftoz(client, z=50, velocity=10):
    print("arming the drone...")
    client.armDisarm(True)
    state = client.getMultirotorState()
    if state.landed_state == airsim.LandedState.Landed:
        print("taking off...")
        client.takeoffAsync().join()
    else:
        client.hoverAsync().join()
    time.sleep(1)
    state = client.getMultirotorState()
    if state.landed_state == airsim.LandedState.Landed:
        print("take off failed...")
        sys.exit(1)
    print("make sure we are hovering at {} meters...".format(-z))
    client.moveToZAsync(-z, 10).join()

def flyamission(client, mission = None, z = 50):
    if mission==None:
        print('Mission empty ! please give a mission plan.')
    else:
        print("Mission start now:")

        result = client.moveOnPathAsync(path=mission, velocity=5, drivetrain=airsim.DrivetrainType.ForwardOnly,
                                        yaw_mode=airsim.YawMode(False,0), lookahead=-1, adaptive_lookahead=0).join()
        while(True):
            print('Now i am detecting the obstacles...' )
            time.sleep(1)
def partrolmission(client, mission = None, z = 50, velocity=8, revert=False):
    if mission==None:
        print('Mission empty ! please give a mission plan.')
    else:
        print("Mission start now:")
        task = client.moveOnPathAsync(mission, velocity, 1200000, airsim.DrivetrainType.ForwardOnly,
                                        airsim.YawMode(False,0), 20, 1)
        while (True):
            time.sleep(1)
            state = client.getMultirotorState()
            t = fabs(state.kinematics_estimated.position.z_val)
            print('Now getting images and obstacle detection ', t)
            if t < z / 2.0 + 12:
                print('Home detected !!!! ', end='')
                break

way = MILP(pathzone,droneAltitute = 35)
path = converttopath(way, 35)
path.append(airsim.Vector3r(0,0, -35))
path.append(airsim.Vector3r(0, 0, -10))

client = airsim.MultirotorClient()
client.confirmConnection()
client.enableApiControl(True)
client.armDisarm(True)
takeofftoz(client,35, 12)
partrolmission(client, path, 35, 7, False)
print('Go home now........ Done !')
client.moveToPositionAsync(0, 0, -10, 12).join()
print("Drone is landing now...................", end='')
client.landAsync().join()
print('Done !')
client.armDisarm(False)
client.enableApiControl(False)
print("All mission is done. Finish !")
