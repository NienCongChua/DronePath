import sys
import cv2
import time
import airsim
import pygame
import shutil

from ultralytics import YOLO
from PIL import Image

model = YOLO("./yolov10n.pt")

Path = 'runs/detect/predict'
shutil.rmtree(Path)

pygame.init()
screen = pygame.display.set_mode((854, 480))
pygame.display.set_caption('Object detection')
screen.fill((0, 0, 0))

AirSim_client = airsim.MultirotorClient()
AirSim_client.confirmConnection()
AirSim_client.enableApiControl(True)

time.sleep(2)
image_types = {
    "scene": airsim.ImageType.Scene,
    "depth": airsim.ImageType.DepthVis,
    "seg": airsim.ImageType.Segmentation,
    "normals": airsim.ImageType.SurfaceNormals,
    "segmentation": airsim.ImageType.Segmentation,
    "disparity": airsim.ImageType.DisparityNormalized
}
base_rate = 0.2
base_throttle = 0.55
speedup_ratio = 4.0
speedup_flag = False
change_time = 0.0
enable_change = True
control_iteration = False
AirSim_client.simSetCameraFov("0", 100)  # FOV càng lớn --> Hình càng nhỏ

scale_percent = 0.5  # 50%

while True:
    pitch_rate = 0.0
    yaw_rate = 0.0
    roll_rate = 0.0
    throttle = base_throttle
    control_iteration = False

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            sys.exit()

    scan_wrapper = pygame.key.get_pressed()

    if scan_wrapper[pygame.K_SPACE]:
        scale_ratio = speedup_ratio
    else:
        scale_ratio = speedup_ratio / speedup_ratio

    if time.time() - change_time > 2:
        enable_change = True

    if scan_wrapper[pygame.K_LCTRL] and scan_wrapper[pygame.K_c] and enable_change:
        enable_change = False
        change_time = time.time()

    if scan_wrapper[pygame.K_a] or scan_wrapper[pygame.K_d]:
        control_iteration = True
        yaw_rate = (scan_wrapper[pygame.K_a] - scan_wrapper[pygame.K_d]) * scale_ratio * base_rate

    if scan_wrapper[pygame.K_UP] or scan_wrapper[pygame.K_DOWN]:
        control_iteration = True
        pitch_rate = (scan_wrapper[pygame.K_UP] - scan_wrapper[pygame.K_DOWN]) * scale_ratio * base_rate

    if scan_wrapper[pygame.K_LEFT] or scan_wrapper[pygame.K_RIGHT]:
        control_iteration = True
        roll_rate = -(scan_wrapper[pygame.K_LEFT] - scan_wrapper[pygame.K_RIGHT]) * scale_ratio * base_rate

    if scan_wrapper[pygame.K_w] or scan_wrapper[pygame.K_s]:
        control_iteration = True
        throttle = base_throttle + (scan_wrapper[pygame.K_w] - scan_wrapper[pygame.K_s]) * scale_ratio * base_rate

    if pitch_rate > 1.0:
        pitch_rate = 1.0
    elif pitch_rate < -1.0:
        pitch_rate = -1.0

    if yaw_rate > 1.0:
        yaw_rate = 1.0
    elif yaw_rate < -1.0:
        yaw_rate = -1.0

    if roll_rate > 1.0:
        roll_rate = 1.0
    elif roll_rate < -1.0:
        roll_rate = -1.0

    if throttle > 1.0:
        throttle = 1.0
    elif throttle < 0.0:
        throttle = 0.0

    temp_image = AirSim_client.simGetImage('0', image_types["scene"])
    if temp_image is None:
        print("Warning: Failed to read a frame!!")
        pygame.quit()
        sys.exit()

    image = cv2.imdecode(airsim.string_to_uint8_array(temp_image), cv2.IMREAD_COLOR)

    # Thêm chức năng thay đổi tỉ lệ hình ảnh từ bàn phím
    if scan_wrapper[pygame.K_PLUS] or scan_wrapper[pygame.K_EQUALS]:
        scale_percent += 0.1
    elif scan_wrapper[pygame.K_MINUS] or scan_wrapper[pygame.K_UNDERSCORE]:
        scale_percent -= 0.1

    # Đảm bảo tỉ lệ hình ảnh nằm trong khoảng hợp lý
    scale_percent = max(0.1, min(1.0, scale_percent))

    # Resize hình ảnh
    width = int(image.shape[1] * scale_percent)
    height = int(image.shape[0] * scale_percent)
    dim = (width, height)
    image = cv2.resize(image, dim, interpolation=cv2.INTER_AREA)

    results = model.predict(source=image, save=True)
    screen_image = pygame.image.load(f'{Path}/image0.jpg')

    screen.blit(screen_image, (0, 0))
    pygame.display.flip()
    pygame.display.update()

    if scan_wrapper[pygame.K_ESCAPE]:
        pygame.quit()
        sys.exit()
