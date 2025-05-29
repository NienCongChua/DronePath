import airsim
import cv2
import numpy as np
import time
from threading import Thread
from queue import Queue

# FPS mục tiêu và khởi tạo queue
target_fps = 200
frame_queue = Queue(maxsize=1)


def get_frame(client, frame_queue):
    while True:
        response = client.simGetImage("1", airsim.ImageType.Scene)
        if response and not frame_queue.full():
            frame_queue.put(response)


# Kết nối với AirSim
client = airsim.MultirotorClient()
client.confirmConnection()

# Khởi tạo cửa sổ và thread
cv2.namedWindow("Drone Camera Stream", cv2.WINDOW_AUTOSIZE)
cv2.startWindowThread()

# Bắt đầu thread lấy frame
frame_thread = Thread(target=get_frame, args=(client, frame_queue))
frame_thread.daemon = True
frame_thread.start()

prev_time = time.time()

try:
    while True:
        current_time = time.time()

        if not frame_queue.empty():
            response = frame_queue.get()
            np_img = np.frombuffer(response, dtype=np.uint8)
            img = cv2.imdecode(np_img, cv2.IMREAD_UNCHANGED)

            if img is not None:
                # Giảm kích thước ảnh nếu cần
                scale_percent = 50
                width = int(img.shape[1] * scale_percent / 100)
                height = int(img.shape[0] * scale_percent / 100)
                img = cv2.resize(img, (width, height), interpolation=cv2.INTER_AREA)

                # Hiển thị frame
                cv2.imshow("Drone Camera Stream", img)

        # Tính toán và hiển thị FPS
        elapsed_time = current_time - prev_time
        actual_fps = 1 / elapsed_time if elapsed_time > 0 else 0
        print(f"FPS: {actual_fps:.2f}", end="\r")

        if cv2.waitKey(1) & 0xFF == 27:
            break

        prev_time = current_time

except KeyboardInterrupt:
    print("\nStopping...")
finally:
    cv2.destroyAllWindows()
