# -*- coding: utf-8 -*-
"""
Created on Tue May 30 10:39:14 2023

@author: ADMIN
"""
import json
import sys
from docplex.mp.model import Model
from openpyxl import Workbook
from openpyxl.utils import get_column_letter
from CPP_data import *
class MILP0:
    #Step 2: importing docplex package
    cpl_model = Model(name='MILP_CPP')
    def __init__(self, data:CPP_data):
        allpoints = data.N + data.NumDepot
        self.X = self.cpl_model.binary_var_cube(data.ND, allpoints, allpoints, name=f'X')
        self.s = self.cpl_model.continuous_var_list(data.N, name=f's')

        self.cst_8 = ((self.s[i] - self.s[j] + data.N * self.X[u, i, j]) <= data.N - 1
                       for u in range(data.ND) for i in range(data.N) for j in (data.NEI[i][1:]) if i != j)

        self.cst_81 = (sum(self.X[u, i, j] for j in range(allpoints)) == 1
                      for i in range(allpoints) for u in range(data.ND))

        self.cst_82 = (sum(self.X[u, i, j] for i in range(allpoints)) == 1
                        for j in range(allpoints) for u in range(data.ND))

        self.cst_83 = (sum(self.X[u, i, j] for j in (data.NEI[i][1:] + list(range(data.N, allpoints))))==1 for i in range(data.N) for u in range(data.ND))

        self.cst_11 = (sum(data.E[i, j] * self.X[u, i, j] for i in range(allpoints) for j in range(allpoints)) <= data.EU[u]
                       for u in range(data.ND))

        self.add_constraints(self.cst_8, self.cst_81, self.cst_82, self.cst_83, self.cst_11)

        self.obj = (sum(data.E[i, j] * self.X[u, i, j] + data.OC[i, j] * self.X[u, i, j]
                        for i in range(allpoints) for j in range(allpoints) for u in range(data.ND)))

        self.cpl_model.set_objective('min', self.obj)

    def add_constraints(self, *constraints):
        for index, constraint in enumerate(constraints):
            self.cpl_model.add_constraints(constraint, names=f'constraint{index}_')

    def solve(self, time_limit=sys.maxsize):
        self.cpl_model.set_time_limit(time_limit)
        self.solution = self.cpl_model.solve(clean_before_solve=True)
        if self.solution == None:
            print('Fails')
        else:
            print('Solve successfull')
    
    def print_results(self):
        print('Objective function value: %.2f' % self.cpl_model.objective_value)
        print(self.cpl_model.solve_details)
    
    def print_solution(self):
        self.cpl_model.print_solution()
        
    def print_information(self):
        self.cpl_model.print_information()
        
    def print_lp_string(self):
        print(self.cpl_model.export_as_lp_string())
        self.cpl_model.export_as_lp('D:/CPP')
        
    def clear_model(self):
        self.cpl_model.clear()
        
    def get_results(self):
        return {'optimal_value':self.cpl_model.objective_value, 
                'time':self.cpl_model.solve_details.time,
                'gap':self.cpl_model.solve_details.gap}
    
    def export_solution_to_file(self, filename, format='json'):
        workbook = Workbook()
        worksheet = workbook.active
        # Write the variable names and solution values as rows
        for variable in self.cpl_model.iter_variables():
            row_data = [variable.name, self.solution[variable]]
            worksheet.append(row_data)
            
        # return self.solution.export(filename, format)
        workbook.save(filename)


    def extractsolution(self, filename, Area, points, z=35, nb=0):
        f = open(filename, mode='w')
        f.write('QGC WPL 110' + '\n')
        f.write('0\t1\t0\t16\t0\t0\t0\t0\t' + str(Area[0].y) + '\t' + str(Area[0].x) + '\t0\t1\n')
        i = 1
        listindex = []
        for variable in self.cpl_model.iter_variables():
            if self.solution[variable] == 1 and variable.name[0] == 'X':
                t = [str(variable.name).split('_')[2], str(variable.name).split('_')[3]]
                listindex.append(t)
        way = [nb]
        for i in range(len(listindex)):
            if int(listindex[i][0]) == int(nb):
                nex = listindex[i][1]
                way.append(int(nex))
                break
        for i in range(1, len(listindex)):
            for j in range(len(listindex)):
                if int(listindex[j][0]) == int(nex):
                    way.append(int(listindex[j][1]))
                    nex = listindex[j][1]
                    break
        darray = []
        darray.append([0, 0, 0, None, Area[0].y, Area[0].x])
        for t in range(len(way)):
            for i in range(len(points)):
                for j in range(len(points[0])):
                    if points[i][j].index == way[t]:
                        stringr = str(t) + '\t0\t3\t16\t0.0\t0.0\t0.0\t0.0\t' + str(points[i][j].y) + '\t' + str(
                            points[i][j].x) + '\t' + str(Area[1].h) + '\t1' + '\n'
                        f.write(stringr)
                        darray.append([0, 0, 0, None, points[i][j].y, points[i][j].x])
        f.close()
        return darray

    def exportToWaypoints(self, filename, Area, points, nb):
        f = open(filename, mode='w')
        f.write('QGC WPL 110' + '\n')
        f.write('0\t1\t0\t16\t0\t0\t0\t0\t' + str(Area[0].y) + '\t' + str(Area[0].x) + '\t0\t1\n')
        i = 1
        listindex = []
        for variable in self.cpl_model.iter_variables():
            if self.solution[variable] == 1 and variable.name[0] == 'X':
                t = [str(variable.name).split('_')[2], str(variable.name).split('_')[3]]
                listindex.append(t)
        way = [nb]
        for i in range(len(listindex)):
            if int(listindex[i][0]) == int(nb):
                nex = listindex[i][1]
                way.append(int(nex))
                break
        for i in range(1, len(listindex)):
            for j in range(len(listindex)):
                if int(listindex[j][0]) == int(nex):
                    way.append(int(listindex[j][1]))
                    nex = listindex[j][1]
                    break
        darray = []
        darray.append([0, 0, 0, None, Area[0].y, Area[0].x])
        for t in range(len(way)):
            for i in range(len(points)):
                for j in range(len(points[0])):
                    if points[i][j].index == way[t]:
                        stringr = str(t) + '\t0\t3\t16\t0.0\t0.0\t0.0\t0.0\t' + str(points[i][j].y) + '\t' + str(
                            points[i][j].x) + '\t' + str(Area[1].h) + '\t1' + '\n'
                        f.write(stringr)
                        darray.append([0, 0, 0, None, points[i][j].y, points[i][j].x])
        f.close()
        print('len', len(darray))
        return darray

    def getsolution(self, Area, points, nb):
        # f = open(filename, mode='w')
        # f.write('QGC WPL 110' + '\n')
        # f.write('0\t1\t0\t16\t0\t0\t0\t0\t' + str(Area[0].y) + '\t' + str(Area[0].x) + '\t0\t1\n')
        i = 1
        listindex = []
        for variable in self.cpl_model.iter_variables():
            if self.solution[variable] == 1 and variable.name[0] == 'X':
                t = [str(variable.name).split('_')[2], str(variable.name).split('_')[3]]
                listindex.append(t)
        way = [nb]
        nex =[]
        for i in range(len(listindex)):
            if int(listindex[i][0]) == int(nb):
                nex = listindex[i][1]
                way.append(int(nex))
                break
        for i in range(1, len(listindex)):
            for j in range(len(listindex)):
                    if int(listindex[j][0]) == int(nex):
                        way.append(int(listindex[j][1]))
                        nex = listindex[j][1]
                        break
        darray = []
        darray.append([0,0,0, None, Area[0].y, Area[0].x])
        for t in range(len(way)):
            for i in range(len(points)):
                for j in range(len(points[0])):
                    if points[i][j].index == way[t]:
                        # stringr = str(t) + '\t0\t3\t16\t0.0\t0.0\t0.0\t0.0\t' + str(points[i][j].y) + '\t' + str(points[i][j].x) + '\t' + str(Area[1].h) + '\t1' +'\n'
                        # f.write(stringr)
                        darray.append([0, 0, 0, None, points[i][j].y, points[i][j].x])
        # f.close()
        return darray

    def exportToJson(self, filename, darray, att=35, speed = 15, hover=5):

        items = []
        for i in range(len(darray)):
            t = darray[i]
            t.append(att)
            commad = 16
            if i == 0: command = 22
            else: command = 16

            item = {
                "AMSLAltAboveTerrain": None,
                "Altitude": att,
                "AltitudeMode": 1,
                "autoContinue": True,
                "command": command,
                "doJumpId": i+1,
                "frame": 3,
                "params": t,
                "type": "SimpleItem"
            }
            items.append(item)
        item20 = {
            "autoContinue": True,
            "command": 20,
            "doJumpId": len(darray)+1,
            "frame": 2,
            "params": [
                0,
                0,
                0,
                0,
                0,
                0,
                0
            ],
            "type": "SimpleItem"
        }
        item21 = {
            "AMSLAltAboveTerrain": None,
            "Altitude": 0,
            "AltitudeMode": 1,
            "autoContinue": True,
            "command": 21,
            "doJumpId": len(darray)+2,
            "frame": 3,
            "params": darray[0],
            "type": "SimpleItem"
        }
        items.append(item20)
        items.append(item21)
        way = {
                "fileType": "Plan",
                "geoFence": {
                "circles": [],
                "polygons": [],
                "version": 2
                },
                "groundStation": "QGroundControl",
                "mission": {
                    "cruiseSpeed": speed,
                    "firmwareType": 12,
                    "globalPlanAltitudeMode": 1,
                    "hoverSpeed": hover,
                "items": items,
                "plannedHomePosition": [items[0]['params'][4], items[0]['params'][5], 0],
                "vehicleType": 2,
                "version": 2
                },
                "rallyPoints": {"points": [],"version": 2},
                "version": 1
        }
        # for i in way:
        #     print(i, ' : ', way[i])
        jsonstring = json.dumps(way,  indent=4)
        with open(filename, "w") as outfile:
            outfile.write(jsonstring)



