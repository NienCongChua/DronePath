import sys
import cv2
import time
import airsim
import pygame
import shutil
import numpy as np
from ultralytics import YOLO
from PIL import Image

# Khởi tạo âm thanh
pygame.mixer.init()
sound = pygame.mixer.Sound("D:/NCKH/GitHubNien/AirsimYolo/AirsimYolo/warning1.mp3")

# Load mô hình YOLO
model = YOLO("D:/NCKH/GitHubNien/AirsimYolo/AirsimYolo/TrainByNien/NCKH/Models/best.pt")

# Xóa thư mục kết quả nếu tồn tại
Path = 'runs/detect/predict'
shutil.rmtree(Path, ignore_errors=True)

# Khởi tạo màn hình pygame với kích thước cố định
pygame.init()
SCREEN_WIDTH, SCREEN_HEIGHT = 1220, 670
screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
pygame.display.set_caption('Object Detection')
screen.fill((0, 0, 0))

# Kết nối AirSim
AirSim_client = airsim.MultirotorClient()
AirSim_client.confirmConnection()
AirSim_client.enableApiControl(True)
AirSim_client.simSetCameraFov("0", 100)

# Thời gian chờ ban đầu
time.sleep(2)

# Định nghĩa kiểu ảnh từ AirSim
image_types = {"scene": airsim.ImageType.Scene}

# Các thông số điều khiển drone
base_rate = 0.2
base_throttle = 0.55

# Ngưỡng confidence để hiển thị bounding box
CONFIDENCE_THRESHOLD = 0.6

# Hàm đọc file phathien.txt và cập nhật allowed_objects
def load_allowed_objects(file_path):
    with open(file_path, "r") as file:
        return {line.strip().lower() for line in file}

# Hàm cập nhật detection_counts dựa trên allowed_objects mới
def update_detection_counts(current_counts, new_allowed_objects):
    updated_counts = {obj: current_counts.get(obj, 0) for obj in new_allowed_objects}
    return updated_counts

# Đường dẫn file phathien.txt
detection_file_path = "D:/NCKH/GitHubNien/AirsimYolo/AirsimYolo/phathien.txt"

# Khởi tạo ban đầu
allowed_objects = load_allowed_objects(detection_file_path)
detection_counts = {obj: 0 for obj in allowed_objects}

# Thời gian kiểm tra file thay đổi (ví dụ: mỗi 1 giây)
last_check_time = time.time()
CHECK_INTERVAL = 1.0  # Kiểm tra mỗi 1 giây

def update_detection_file():
    try:
        with open("D:/NCKH/GitHubNien/DronePath-main/DronePath-main/DroneNien/source/detect/detection_counts.txt", "w") as file:
            for obj, count in detection_counts.items():
                file.write(f"{obj}: {count}\n")
    except:
        pass

while True:
    pitch_rate = yaw_rate = roll_rate = 0.0
    throttle = base_throttle

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            update_detection_file()
            pygame.quit()
            sys.exit()

    # Kiểm tra và cập nhật allowed_objects nếu file thay đổi
    current_time = time.time()
    if current_time - last_check_time >= CHECK_INTERVAL:
        new_allowed_objects = load_allowed_objects(detection_file_path)
        if new_allowed_objects != allowed_objects:
            allowed_objects = new_allowed_objects
            detection_counts = update_detection_counts(detection_counts, allowed_objects)
        last_check_time = current_time

    scan_wrapper = pygame.key.get_pressed()

    # Điều khiển drone
    if scan_wrapper[pygame.K_a] or scan_wrapper[pygame.K_d]:
        yaw_rate = (scan_wrapper[pygame.K_a] - scan_wrapper[pygame.K_d]) * base_rate

    if scan_wrapper[pygame.K_UP] or scan_wrapper[pygame.K_DOWN]:
        pitch_rate = (scan_wrapper[pygame.K_UP] - scan_wrapper[pygame.K_DOWN]) * base_rate

    if scan_wrapper[pygame.K_LEFT] or scan_wrapper[pygame.K_RIGHT]:
        roll_rate = -(scan_wrapper[pygame.K_LEFT] - scan_wrapper[pygame.K_RIGHT]) * base_rate

    if scan_wrapper[pygame.K_w] or scan_wrapper[pygame.K_s]:
        throttle = base_throttle + (scan_wrapper[pygame.K_w] - scan_wrapper[pygame.K_s]) * base_rate

    throttle = np.clip(throttle, 0.0, 1.0)
    pitch_rate = np.clip(pitch_rate, -1.0, 1.0)
    yaw_rate = np.clip(yaw_rate, -1.0, 1.0)
    roll_rate = np.clip(roll_rate, -1.0, 1.0)

    # Lấy ảnh từ AirSim
    temp_image = AirSim_client.simGetImage('0', image_types["scene"])
    if temp_image is None:
        print("Warning: Failed to read a frame!")
        update_detection_file()
        pygame.quit()
        sys.exit()

    image = cv2.imdecode(airsim.string_to_uint8_array(temp_image), cv2.IMREAD_COLOR)
    image = cv2.resize(image, (SCREEN_WIDTH, SCREEN_HEIGHT), interpolation=cv2.INTER_AREA)

    # Nhận diện đối tượng
    results = model.predict(source=image, save=False, verbose=False)

    detected_classes = set()
    for result in results:
        for box in result.boxes:
            confidence = box.conf[0].item()
            if confidence > CONFIDENCE_THRESHOLD:
                x1, y1, x2, y2 = map(int, box.xyxy[0])
                class_id = int(box.cls[0])
                label = model.names[class_id].lower()

                # Chỉ xử lý nếu object nằm trong danh sách được chọn
                if label in allowed_objects:
                    detected_classes.add(class_id)
                    detection_counts[label] = detection_counts.get(label, 0) + 1
                    update_detection_file()  # Cập nhật file ngay lập tức
                    cv2.rectangle(image, (x1, y1), (x2, y2), (0, 255, 0), 2)
                    cv2.putText(image, f"{label} {confidence:.2f}", (x1, y1 - 10),
                                cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

    # Phát âm thanh nếu phát hiện đối tượng
    if detected_classes:
        if not pygame.mixer.get_busy():
            sound.play()
    else:
        sound.stop()

    # Hiển thị ảnh
    img_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    img_pil = Image.fromarray(img_rgb)
    img_pygame = pygame.image.fromstring(img_pil.tobytes(), img_pil.size, img_pil.mode)
    screen.blit(img_pygame, (0, 0))
    pygame.display.flip()

    if scan_wrapper[pygame.K_ESCAPE]:
        update_detection_file()
        pygame.quit()
        sys.exit()