import airsim
import cv2
import numpy as np
import socket

# Kết nối AirSim
client = airsim.MultirotorClient()
client.confirmConnection()

# Kết nối socket tới WPF
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(("localhost", 4999))

while True:
    # Lấy ảnh từ camera
    response = client.simGetImage("0", airsim.ImageType.Scene)
    if response:
        np_img = np.frombuffer(response, dtype=np.uint8)
        img = cv2.imdecode(np_img, cv2.IMREAD_COLOR)

        # Mã hóa ảnh thành JPEG
        _, img_encoded = cv2.imencode('.jpg', img)
        sock.sendall(img_encoded.tobytes())

    # Hiển thị video trong Python (tùy chọn)
    cv2.imshow("Drone Camera Stream", img)
    if cv2.waitKey(1) & 0xFF == 27:
        break

cv2.destroyAllWindows()
sock.close()
