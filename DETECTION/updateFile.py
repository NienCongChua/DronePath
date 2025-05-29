def update_detection_file(hehe):
    with open("D:/NCKH/GitHubNien/DronePath-main/DronePath-main/DroneNien/source/detect/detection_counts.txt", "w") as file:
        for obj, count in hehe.items():
            file.write(f"{obj}: {count}\n")

# Đọc danh sách đối tượng từ file nhandang.txt
with open("D:/NCKH/GitHubNien/AirsimYolo/AirsimYolo/phathien.txt", "r") as file:
    allowed_objects = {line.strip().lower() for line in file}
    # Dictionary to keep track of detection counts
    detection_counts = {obj: 0 for obj in allowed_objects}
    update_detection_file(detection_counts)