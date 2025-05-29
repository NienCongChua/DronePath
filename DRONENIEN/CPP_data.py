import numpy as np
import pandas as pd
class CPP_data:
    E:np.ndarray
    OC:np.ndarray
    EU:np.ndarray
    N:int
    ND:int

    def __init__(self, N, ND = 1, NDP = 1):
        self.N = N
        self.ND = ND
        self.NumDepot = NDP #Number of deport
        self.E = np.random.rand(N, N)
        self.OC = np.zeros((N+ND, N+ND))
        self.EU = np.zeros(ND)
        self.NEI = list(np.zeros(N + NDP))
        self.ANG = list(np.zeros(N + NDP))
        self.E = np.zeros((N+NDP, N + NDP))
        self.r = np.zeros((N+NDP, N + NDP))
        self.R = list(np.zeros(N + NDP))
        self.Turn = np.array([0, 0.1, 0.2])
        self.Res = np.zeros([self.ND])
        self.Res[0] = 2000000

    def read_data(self, filename1, filename2 = None):
        f = open(filename1, 'r')
        for i in range(self.N):
            self.E[i] = np.array(list(map(float, f.readline().split())))
        f.close()
        if filename2 != 'None':
            f = open(filename2, 'r')
            for i in range(self.N):
                self.OC[i] = np.array(list(map(float, f.readline().split())))
            f.close()

    def read_data_MILP3(self, filename1, filename2 = None, filename3 = None):
        f = open(filename1, 'r')
        for i in range(self.N + self.NumDepot):
            self.NEI[i] = list(map(int, f.readline().split()))
        f.close()

        if filename2 != 'None':
            f = open(filename2, 'r')
            for i in range(self.N + self.NumDepot):
                self.ANG[i] = list(map(int, f.readline().split()))
            f.close()

        if filename3 != 'None':
            f = open(filename3, 'r')
            for i in range(self.N + self.NumDepot):
                self.r[i] = np.array(list(map(float, f.readline().split())))
            f.close()


    def set_eu(self, eu):
        for i in range(self.ND):
            self.EU[i] = eu[i]

    def generateR(self):
        if len(self.NEI) > 0:
            for i in range(self.N + self.NumDepot):
                self.R[i] = [0.0] * (self.N + self.NumDepot)

            for i in range(self.N):
                for j in range(self.N):
                    self.R[i][j] = self.Turn[0]
                    if self.ANG[i][j] > 0: #có đường từ i sang j
                        First = self.ANG[i][j] + 2
                        End = self.ANG[i][j] - 2
                        if End == 0: End = 6
                        if End == -1: End = 5
                        if First == 7: First = 1
                        if First == 8: First = 2

                        for k in self.NEI[i][1:]:
                            if (self.ANG[i][k] == First or self.ANG[i][k] == End) and self.R[i][j] == self.Turn[0]:
                                self.R[i][j] = self.Turn[1]

                        First = self.ANG[i][j] + 1
                        End = self.ANG[i][j] - 1
                        if End == 0: End = 6
                        if First == 7: First = 1
                        for k in self.NEI[i][1:]:
                            if (self.ANG[i][k] == First or self.ANG[i][k] == End) and self.R[i][j] < self.Turn[2]:
                                self.R[i][j] = self.Turn[2]
            #np.savetxt('D:/R.txt', self.R)



