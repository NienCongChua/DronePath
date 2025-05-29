
from math import *
import numpy as np
#from dronekit import connect, Command, VehicleMode, LocationGlobal, LocationGlobalRelative
import time
import math

from dronekit import Command, LocationGlobal, LocationGlobalRelative, VehicleMode
import json
MeterOfLat = 111111.0
MeterOfLon = 74779.2
# FootPrintWidth = 50.0

# MeterOfLon = 74779.2
FootPrintWidth = 60.0

#from pymavlink import mavutil
import matplotlib.pyplot as plt
from Point import *
def exporttofile(filename , matrix):
    f = open(filename, mode='w')
    for i in range(len(matrix)):
        for j in range(len(matrix[i])):
            f.write(str(matrix[i][j]) + ' ')
        f.write('\n')
    f.close()
def json2darray(pathjson, pathway, att=50):
    data = json.loads(open(pathjson, "r").read())
    items = data['mission']['items']
    darray = []
    for index, value in enumerate(items):
        darray.append(value['params'])

    f = open(pathway, 'w', encoding='utf-8')
    f.write('QGC WPL 110' + '\n')
    f.write('0\t1\t0\t16\t0\t0\t0\t0\t' + str(darray[0][4]) + '\t' + str(darray[0][5]) + '\t0\t1\n')
    for i in range(len(darray)):
        if darray[i][6] > 0:
            f.write('0\t0\t3\t16\t0\t0\t0\t0\t' + str(darray[i][4]) + '\t' + str(darray[i][5]) + '\t' + str(darray[i][6]) + '\t1\n')
    f.write('0\t0\t3\t16\t0\t0\t0\t0\t' + str(darray[0][4]) + '\t' + str(darray[0][5]) + '\t0\t1\n')
    f.close()
    return darray

def readmission(vehicle, aFileName):
    print("\nReading mission from file: %s" % aFileName)
    cmds = vehicle.commands
    missionlist = []
    with open(aFileName) as f:
        for i, line in enumerate(f):
            if i == 0:
                if not line.startswith('QGC WPL 110'):
                    raise Exception('File is not supported WP version')
            else:
                linearray = line.split('\t')
                ln_index = int(linearray[0])
                ln_currentwp = int(linearray[1])
                ln_frame = int(linearray[2])
                ln_command = int(linearray[3])
                ln_param1 = float(linearray[4])
                ln_param2 = float(linearray[5])
                ln_param3 = float(linearray[6])
                ln_param4 = float(linearray[7])
                ln_param5 = float(linearray[8])
                ln_param6 = float(linearray[9])
                ln_param7 = float(linearray[10])
                ln_autocontinue = int(linearray[11].strip())
                cmd = Command(0, 0, 0, ln_frame, ln_command, ln_currentwp, ln_autocontinue, ln_param1, ln_param2,
                              ln_param3, ln_param4, ln_param5, ln_param6, ln_param7)
                missionlist.append(cmd)
    return missionlist

def GenMissionArea(plan):
    Area = []
    Area.append(Point(plan[0][5], plan[0][4], 0, True))
    maxlat = -100000.0
    maxlon = -100000.0
    minlat = 100000.0
    minlon = 100000.0
    for t in range(1, len(plan)):
        q = Point(x=plan[t][5], y=plan[t][4], h=plan[t][6], use=True)
        Area.append(q)
        if q.y > maxlat: maxlat = q.y
        if q.x > maxlon: maxlon = q.x
        if q.y < minlat: minlat = q.y
        if q.x < minlon: minlon = q.x

    points = []
    contour = FootPrintWidth / MeterOfLat / 2

    nblat = int((maxlat - minlat) * MeterOfLat / FootPrintWidth)
    nblon = int((maxlon - minlon) * MeterOfLat / FootPrintWidth)
    high = Area[1].h
    for i in range(nblat):
        row = [Point(0, 0, 0, True)] * nblon
        for j in range(nblon):
            row[j] = Point(minlon + contour + j * (maxlon - minlon) / nblon, minlat + contour + i * (maxlat - minlat) / nblat, high,
                               True)
        points.append(row)
    return Area, points

def ReadMissionArea(filename):
  Area = []
  f = open(filename, mode='r')
  k = f.readline()
  if not k.startswith('QGC WPL 110'):
      print('Mission area file is not valid format !')
      return 0, 0
  else:
      maxlat = -10000.0
      maxlon = -10000.0
      minlat = 10000.0
      minlon = 10000.0
      k = f.readlines()
      for i in range(len(k)):
          t = list(map(float, k[i].split('\t')))
          q = Point(t[9], t[8], t[10], True)
          Area.append(q)
          if q.y > maxlat: maxlat = q.y
          if q.x > maxlon: maxlon = q.x
          if q.y < minlat: minlat = q.y
          if q.x < minlon: minlon = q.x

      points = []
      contourlat = FootPrintWidth/MeterOfLat/2
      contourlon = FootPrintWidth/MeterOfLat/2

      nblat = int((maxlat-minlat)*MeterOfLat/FootPrintWidth)
      nblon = int((maxlon-minlon)*MeterOfLat/FootPrintWidth)
      high = Area[1].h
      for i in range(nblat):
          row = [Point(0, 0, 0, True)] * nblon
          for j in range(nblon):
              if j%2==1:
                  row[j] = Point(minlon + j * (maxlon - minlon) / nblon, minlat  + contourlat + i*(maxlat-minlat)/nblat, high, True)
              else:
                  row[j] = Point(minlon + j * (maxlon - minlon) / nblon, minlat +  i * (maxlat - minlat) / nblat, high,
                                 True)
          points.append(row)
      return Area, points

def checkPoints(Area, points, obstacles = None):
      polygon = Area[1:]
      n = len(polygon)
      for j in range(len(points[1])):
          for i in range(len(points)):
              points[i][j].use = checkInside(polygon, n, points[i][j])

      if obstacles != None:
          for i in range(len(obstacles)):
              points[obstacles[i][0]][obstacles[i][1]].use = 0
      return points

def getIndex(points):
    ind  = 0
    for j in range(len(points[0])):
        for i in range(len(points)):
            if points[i][j].use == True:
                points[i][j].index = ind
                ind = ind + 1
    NEI = []
    angsize = ind + 1
    ANG = []
    E = []
    for i in range(angsize):
        ANG.append([0]*angsize)
    for j in range(len(points[0])):
        for i in range(len(points)):
            if points[i][j].use == 1:
                k = [points[i][j].index]
                if i-1 >= 0 and points[i-1][j].use:
                    k.append(points[i-1][j].index)
                    ANG[points[i][j].index][points[i-1][j].index] = 1
                if i-1>= 0 and j-1 >= 0 and points[i-1][j-1].use==1:
                    k.append(points[i-1][j-1].index)
                    ANG[points[i][j].index][points[i-1][j-1].index] = 2
                if j-1 >= 0 and points[i][j-1].use:
                    k.append(points[i][j-1].index)
                    ANG[points[i][j].index][points[i][j-1].index] = 3
                if i+1 <len(points) and j-1 >= 0 and points[i+1][j-1].use:
                    k.append(points[i+1][j-1].index)
                    ANG[points[i][j].index][points[i+1][j-1].index] = 4
                if i + 1 < len(points) and points[i+1][j].use:
                    k.append(points[i+1][j].index)
                    ANG[points[i][j].index][points[i+1][j].index] = 5
                if i+1 < len(points) and j+1 < len(points[0]) and points[i+1][j+1].use:
                    k.append(points[i+1][j+1].index)
                    ANG[points[i][j].index][points[i+1][j+1].index] = 6
                if j+1 < len(points[0]) and points[i][j+1].use:
                    k.append(points[i][j+1].index)
                    ANG[points[i][j].index][points[i][j+1].index] = 7
                if i-1 >=0 and j+1 < len(points[0]) and points[i-1][j+1].use:
                    k.append(points[i-1][j+1].index)
                    ANG[points[i][j].index][points[i-1][j+1].index] = 8


                NEI.append(k)
    return NEI, ANG


def distancetwopoints(points, index1, index2):
    A = None
    B = None
    for i in range(len(points)):
        for j in range(len(points[0])):
            if points[i][j].index == index1: A = points[i][j]
            if points[i][j].index == index2: B = points[i][j]
    if A != None and B != None:
        return sqrt(((A.x - B.x) ** 2) + ((A.y - B.y) ** 2)) * MeterOfLat
    return


def distancefromdepot(points, depot, index1):
    A = depot
    B = None

    for i in range(len(points)):
        for j in range(len(points[0])):
            if points[i][j].index == index1: B = points[i][j]

    if B != None:
        return sqrt(((A.x - B.x) ** 2)*MeterOfLat + ((A.y - B.y) ** 2) * MeterOfLat)
    return



def addDepot(NEI, ANG, nbdepot):
    n = len(NEI)
    ang = []
    for i in range(n+nbdepot):
        ang.append([0]* (n+nbdepot))

    for i in range(len(ANG)):
        for j in range(len(ANG[0])):
            ang[i][j] = ANG[i][j]

    for i in range(nbdepot):
        row = [n+i] + [t for t in range(n)]
        NEI.append(row)
    return NEI, ang

def adddistancefromdepot(points, depot, nb):
    dis = []
    B = None
    for t in range(nb):
        for i in range(len(points)):
            for j in range(len(points[0])):
                if points[i][j].index == t:
                    B = points[i][j]
        if B != None:
            dis.append(10000 + sqrt(((depot.x - B.x) ** 2) + ((depot.y - B.y) ** 2))*MeterOfLat)
        else:
            dis.append(-1)
    return dis


def distancematrix(points, ANG, nb):
    dis = np.zeros((nb, nb))
    for i in range(nb):
        for j in range(nb):
            if ANG[i][j] > 0:
                dis[i, j] = distancetwopoints(points, i, j)
    return dis


def addcolumn(matrix, column):

    n = len(matrix)
    ma = np.zeros((n+1, n+1))
    for i in range(n):
        for j in range(n):
            ma[i][j] = matrix[i][j]
        ma[i][n] = column[i]
        ma[n][i] = column[i]
    return ma




def upload_mission(vehicle, aFileName):
    missionlist = readmission(vehicle, aFileName)
    print("\nUpload mission from a file: %s" % aFileName)
    # Clear existing mission from vehicle
    cmds = vehicle.commands
    cmds.clear()
    # Add new mission to vehicle
    for command in missionlist:
        cmds.add(command)
    print('Upload mission to vehicle now ....')
    vehicle.commands.upload()


def download_mission(vehicle):
    print(" Download mission from vehicle")
    missionlist = []
    cmds = vehicle.commands
    cmds.download()
    cmds.wait_ready()
    for cmd in cmds:
        missionlist.append(cmd)
    return missionlist


def save_mission(vehicle, aFileName):
    print("\nSave mission from Vehicle to file: %s" % aFileName)
    # Download mission from vehicle
    missionlist = download_mission()
    # Add file-format information
    output = 'QGC WPL 110\n'
    # Add home location as 0th waypoint
    home = vehicle.home_location
    output += "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n" % (
    0, 1, 0, 16, 0, 0, 0, 0, home.lat, home.lon, home.alt, 1)
    # Add commands
    for cmd in missionlist:
        commandline = "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n" % (
        cmd.seq, cmd.current, cmd.frame, cmd.command, cmd.param1, cmd.param2, cmd.param3, cmd.param4, cmd.x, cmd.y,
        cmd.z, cmd.autocontinue)
        output += commandline
    with open(aFileName, 'w') as file_:
        print(" Write mission to file")
        file_.write(output)


def printfile(aFileName):
    print("\nMission file: %s" % aFileName)
    with open(aFileName) as f:
        for line in f:
            print(' %s' % line.strip())

def arm_and_takeoff(vehicle, aTargetAltitude):
    print("Basic pre-arm checks")
    while not vehicle.is_armable:
        print(" Waiting for vehicle to initialise...")
        time.sleep(1)
    print("Arming motors")
    vehicle.mode = VehicleMode("GUIDED")
    vehicle.armed = True
    while not vehicle.armed:
        print(" Waiting for arming...")
        time.sleep(1)
    print("Taking off!")
    vehicle.simple_takeoff(aTargetAltitude)
    while True:
        print(" Altitude: ", vehicle.location.global_relative_frame.alt)
        # Break and return from function just below target altitude.
        if vehicle.location.global_relative_frame.alt >= aTargetAltitude * 0.95:
            print("Reached target altitude")
            break
        time.sleep(1)



def get_location_metres(original_location, dNorth, dEast):
    earth_radius=6378137.0
    dLat = dNorth/earth_radius
    dLon = dEast/(earth_radius*math.cos(math.pi*original_location.lat/180))

    #New position in decimal degrees
    newlat = original_location.lat + (dLat * 180/math.pi)
    newlon = original_location.lon + (dLon * 180/math.pi)
    return LocationGlobal(newlat, newlon,original_location.alt)


def get_distance_metres(aLocation1, aLocation2):
    dlat = aLocation2.lat - aLocation1.lat
    dlong = aLocation2.lon - aLocation1.lon
    return math.sqrt((dlat*dlat) + (dlong*dlong)) * 1.113195e5



def distance_to_current_waypoint(vehicle):
    nextwaypoint = vehicle.commands.next
    if nextwaypoint==0:
        return None
    missionitem=vehicle.commands[nextwaypoint-1] #commands are zero indexed
    lat = missionitem.x
    lon = missionitem.y
    alt = missionitem.z
    targetWaypointLocation = LocationGlobalRelative(lat,lon,alt)
    distancetopoint = get_distance_metres(vehicle.location.global_frame, targetWaypointLocation)
    return distancetopoint
