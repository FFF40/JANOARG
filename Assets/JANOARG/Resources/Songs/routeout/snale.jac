JANOARG Chart Format
github.com/FFF40/JANOARG

[VERSION]
1

[METADATA]
Index: -1
Name: 🐌
Charter: duducat
Level: ??
Constant: 0

[CAMERA]
Pivot: 0 0.5 0
Rotation: 15 0 0
Distance: 10
$ CameraRotation_X 0b 4 0 _ Quadratic/InOut
$ CameraPivot_Y 0b 4 0 _ Quadratic/InOut
$ CameraRotation_Z 4b 4 -25 _ Quadratic/InOut
$ CameraRotation_X 8b 4 10 _ Quadratic/InOut
$ CameraRotation_Z 12b 4 0 _ Quadratic/InOut
$ CameraRotation_X 12b 4 20 _ Quadratic/InOut
$ CameraPivot_Y 12b 4 1 _ Quadratic/InOut
$ CameraRotation_X 16b 8 0 _ Quadratic/InOut
$ CameraPivot_Y 16b 8 1.25 _ Quadratic/InOut
$ CameraRotation_X 92b 8 25 _ Quadratic/InOut
$ CameraRotation_X 104b 8 0 _ Quadratic/InOut

[GROUPS]

[PALLETE]
Background: 0.9528909 1 0.8726415 1
Interface: 0.005984667 0 0.2830189 1
+ LaneStyle 0 0.04928492 0.8584906 0.6 0 0.1720123 1 1
+ HitStyle 0 0.01973684 0.15 0.8 0 0 0 1 0 0 1 1

[OBJECTS]
+ Lane 0 -3 0 0 0 0 0
  + LaneStep -4b 0 0 Linear Linear 0 0 Linear Linear 1
  + LaneStep 0b -5 0 Linear Linear 5 0 Linear Linear 1
  + LaneStep 16b -5 0 Linear Linear 5 0 Linear Linear 0.01
  + Hit Normal 0b 0.05 0.4 16 N 0
  + Hit Normal 0b 0.55 0.4 16 N 0
+ Lane 0 3 0 0 0 0 0
  $ Offset_X 4b 4 -4 _ Quadratic/InOut
  $ OffsetRotation_Z 4b 4 40 _ Quadratic/InOut
  + LaneStep 0b -3 0 Linear Linear 3 0 Linear Linear 1
  + LaneStep 16b -3 0 Linear Linear 3 0 Linear Linear 0.01
  + Hit Normal 4b 0.1 0.8 12 N 0
+ Lane 5 0 0 0 0 -45 0
  $ Offset_X 8b 4 -3 _ Quadratic/InOut
  $ OffsetRotation_Z 8b 4 15 _ Quadratic/InOut
  + LaneStep 4b -3 0 Linear Linear 3 0 Linear Linear 1
  + LaneStep 16b -3 0 Linear Linear 3 0 Linear Linear 0.01
  + Hit Normal 8b 0.1 0.8 8 N 0
+ Lane 12 1 0 0 0 -60 0
  $ Offset_X 8b 4 4 _ Quadratic/InOut
  + LaneStep 8b -3 0 Linear Linear 3 0 Linear Linear 1
  + LaneStep 16b -3 0 Linear Linear 3 0 Linear Linear 0.01
  + Hit Normal 12b 0.1 0.8 4 N 0
+ Lane 0 -3 0 0 0 0 0
  + LaneStep 16b -6 0 Linear Linear 6 0 Linear Linear 0.25
  + LaneStep 44b -6 0 Linear Linear 6 0 Linear Linear 0.05
  + LaneStep 48b -6 0 Linear Linear 6 0 Linear Linear 0.01
  + Hit Normal 16b 0.1 0.35 0 N 0
  + Hit Normal 16b1/4 0.55 0.35 0 N 0
  + Hit Normal 16b1/2 0.3 0.35 0 N 0
  + Hit Normal 16b3/4 0.55 0.35 0 N 0
  + Hit Normal 17b 0.3 0.35 0 N 0
  + Hit Normal 17b1/4 0.55 0.35 0 N 0
  + Hit Normal 17b1/2 0.3 0.35 0 N 0
  + Hit Normal 17b3/4 0.55 0.35 0 N 0
  + Hit Normal 18b 0.55 0.35 0 N 0
  + Hit Normal 18b 0.1 0.35 0 N 0
  + Hit Normal 18b1/4 0.3 0.35 0 N 0
  + Hit Normal 18b1/2 0.55 0.35 0 N 0
  + Hit Normal 18b3/4 0.3 0.35 0 N 0
  + Hit Normal 19b 0.55 0.35 0 N 0
  + Hit Normal 19b1/4 0.3 0.35 0 N 0
  + Hit Normal 19b1/2 0.55 0.35 0 N 0
  + Hit Normal 19b3/4 0.1 0.35 0 N 0
  + Hit Normal 20b 0.55 0.35 0 N 0
  + Hit Normal 20b 0.1 0.35 0 N 0
  + Hit Normal 20b1/4 0.55 0.35 0 N 0
  + Hit Normal 20b1/2 0.3 0.35 0 N 0
  + Hit Normal 20b3/4 0.55 0.35 0 N 0
  + Hit Normal 21b 0.3 0.35 0 N 0
  + Hit Normal 21b1/4 0.55 0.35 0 N 0
  + Hit Normal 21b1/2 0.3 0.35 0 N 0
  + Hit Normal 21b3/4 0.55 0.35 0 N 0
  + Hit Normal 22b 0.55 0.35 0 N 0
  + Hit Normal 22b 0.1 0.35 0 N 0
  + Hit Normal 22b1/4 0.3 0.35 0 N 0
  + Hit Normal 22b1/2 0.55 0.35 0 N 0
  + Hit Normal 22b3/4 0.3 0.35 0 N 0
  + Hit Normal 23b 0.55 0.35 0 N 0
  + Hit Normal 23b1/4 0.3 0.35 0 N 0
  + Hit Normal 23b1/2 0.55 0.35 0 N 0
  + Hit Normal 23b3/4 0.1 0.35 0 N 0
  + Hit Normal 24b 0.55 0.35 0 N 0
  + Hit Normal 24b 0.1 0.35 0 N 0
  + Hit Normal 24b1/4 0.55 0.35 0 N 0
  + Hit Normal 24b1/2 0.3 0.35 0 N 0
  + Hit Normal 24b3/4 0.55 0.35 0 N 0
  + Hit Normal 25b 0.3 0.35 0 N 0
  + Hit Normal 25b1/4 0.55 0.35 0 N 0
  + Hit Normal 25b1/2 0.3 0.35 0 N 0
  + Hit Normal 25b3/4 0.55 0.35 0 N 0
  + Hit Normal 26b 0.55 0.35 0 N 0
  + Hit Normal 26b 0.1 0.35 0 N 0
  + Hit Normal 26b1/4 0.3 0.35 0 N 0
  + Hit Normal 26b1/2 0.55 0.35 0 N 0
  + Hit Normal 26b3/4 0.3 0.35 0 N 0
  + Hit Normal 27b 0.55 0.35 0 N 0
  + Hit Normal 27b1/4 0.3 0.35 0 N 0
  + Hit Normal 27b1/2 0.55 0.35 0 N 0
  + Hit Normal 27b3/4 0.1 0.35 0 N 0
  + Hit Normal 28b 0.55 0.35 0 N 0
  + Hit Normal 28b 0.1 0.35 0 N 0
  + Hit Normal 28b1/4 0.55 0.35 0 N 0
  + Hit Normal 28b1/2 0.3 0.35 0 N 0
  + Hit Normal 28b3/4 0.55 0.35 0 N 0
  + Hit Normal 29b 0.3 0.35 0 N 0
  + Hit Normal 29b1/4 0.55 0.35 0 N 0
  + Hit Normal 29b1/2 0.3 0.35 0 N 0
  + Hit Normal 29b3/4 0.55 0.35 0 N 0
  + Hit Normal 30b 0.55 0.35 0 N 0
  + Hit Normal 30b 0.1 0.35 0 N 0
  + Hit Normal 30b1/4 0.3 0.35 0 N 0
  + Hit Normal 30b1/2 0.55 0.35 0 N 0
  + Hit Normal 30b3/4 0.3 0.35 0 N 0
  + Hit Normal 31b 0.55 0.35 0 N 0
  + Hit Normal 31b1/4 0.3 0.35 0 N 0
  + Hit Normal 31b1/2 0.55 0.35 0 N 0
  + Hit Normal 31b3/4 0.1 0.35 0 N 0
  + Hit Normal 32b 0.55 0.35 0 N 0
  + Hit Normal 32b 0.1 0.35 0 N 0
  + Hit Normal 32b1/4 0.55 0.35 0 N 0
  + Hit Normal 32b1/2 0.3 0.35 0 N 0
  + Hit Normal 32b3/4 0.55 0.35 0 N 0
  + Hit Normal 33b 0.3 0.35 0 N 0
  + Hit Normal 33b1/4 0.55 0.35 0 N 0
  + Hit Normal 33b1/2 0.3 0.35 0 N 0
  + Hit Normal 33b3/4 0.55 0.35 0 N 0
  + Hit Normal 34b 0.55 0.35 0 N 0
  + Hit Normal 34b 0.1 0.35 0 N 0
  + Hit Normal 34b1/4 0.3 0.35 0 N 0
  + Hit Normal 34b1/2 0.55 0.35 0 N 0
  + Hit Normal 34b3/4 0.3 0.35 0 N 0
  + Hit Normal 35b 0.55 0.35 0 N 0
  + Hit Normal 35b1/4 0.3 0.35 0 N 0
  + Hit Normal 35b1/2 0.55 0.35 0 N 0
  + Hit Normal 35b3/4 0.1 0.35 0 N 0
  + Hit Normal 36b 0.55 0.35 0 N 0
  + Hit Normal 36b 0.1 0.35 0 N 0
  + Hit Normal 36b1/4 0.55 0.35 0 N 0
  + Hit Normal 36b1/2 0.3 0.35 0 N 0
  + Hit Normal 36b3/4 0.55 0.35 0 N 0
  + Hit Normal 37b 0.3 0.35 0 N 0
  + Hit Normal 37b1/4 0.55 0.35 0 N 0
  + Hit Normal 37b1/2 0.3 0.35 0 N 0
  + Hit Normal 37b3/4 0.55 0.35 0 N 0
  + Hit Normal 38b 0.55 0.35 0 N 0
  + Hit Normal 38b 0.1 0.35 0 N 0
  + Hit Normal 38b1/4 0.3 0.35 0 N 0
  + Hit Normal 38b1/2 0.55 0.35 0 N 0
  + Hit Normal 38b3/4 0.3 0.35 0 N 0
  + Hit Normal 39b 0.55 0.35 0 N 0
  + Hit Normal 39b1/4 0.3 0.35 0 N 0
  + Hit Normal 39b1/2 0.55 0.35 0 N 0
  + Hit Normal 39b3/4 0.1 0.35 0 N 0
  + Hit Normal 40b 0.55 0.35 0 N 0
  + Hit Normal 40b 0.1 0.35 0 N 0
  + Hit Normal 40b1/4 0.55 0.35 0 N 0
  + Hit Normal 40b1/2 0.3 0.35 0 N 0
  + Hit Normal 40b3/4 0.55 0.35 0 N 0
  + Hit Normal 41b 0.3 0.35 0 N 0
  + Hit Normal 41b1/4 0.55 0.35 0 N 0
  + Hit Normal 41b1/2 0.3 0.35 0 N 0
  + Hit Normal 41b3/4 0.55 0.35 0 N 0
  + Hit Normal 42b 0.55 0.35 0 N 0
  + Hit Normal 42b 0.1 0.35 0 N 0
  + Hit Normal 42b1/4 0.3 0.35 0 N 0
  + Hit Normal 42b1/2 0.55 0.35 0 N 0
  + Hit Normal 42b3/4 0.3 0.35 0 N 0
  + Hit Normal 43b 0.55 0.35 0 N 0
  + Hit Normal 43b1/4 0.3 0.35 0 N 0
  + Hit Normal 43b1/2 0.55 0.35 0 N 0
  + Hit Normal 43b3/4 0.1 0.35 0 N 0
  + Hit Normal 44b 0.55 0.35 4 N 0
  + Hit Normal 44b 0.1 0.35 4 N 0
+ Lane -6 5 0 0 0 60 0
  + LaneStep 16b -2 0 Linear Linear 2 0 Linear Linear 1
  + LaneStep 48b -2 0 Linear Linear 2 0 Linear Linear 0.01
  + Hit Normal 22b3/4 0.05 0.9 0 N 0
  + Hit Normal 23b3/4 0.05 0.9 0 N 0
  + Hit Normal 26b 0.05 0.9 0.75 N 0
  + Hit Normal 27b2/3 0.05 0.9 0 N 0
  + Hit Normal 30b 0.05 0.9 1 N 0
  + Hit Normal 34b1/2 0.05 0.9 0.5 N 0
  + Hit Normal 38b 0.05 0.9 1 N 0
  + Hit Normal 43b 0.05 0.9 0.5 N 0
+ Lane 6 5 0 0 0 120 0
  + LaneStep 16b -2 0 Linear Linear 2 0 Linear Linear 1
  + LaneStep 48b -2 0 Linear Linear 2 0 Linear Linear 0.01
  + Hit Normal 23b 0.05 0.9 0 N 0
  + Hit Normal 24b 0.05 0.9 0 N 0
  + Hit Normal 24b 0.05 0.9 2 N 0
  + Hit Normal 27b5/6 0.05 0.9 0 N 0
  + Hit Normal 31b 0.05 0.9 1 N 0
  + Hit Normal 35b 0.05 0.9 0.5 N 0
  + Hit Normal 42b 0.05 0.9 1 N 0
  + Hit Normal 43b1/2 0.05 0.9 0.5 N 0
+ Lane -7 0 0 0 0 90 0
  + LaneStep 16b -2 0 Linear Linear 2 0 Linear Linear 1
  + LaneStep 48b -2 0 Linear Linear 2 0 Linear Linear 0.01
  + Hit Normal 18b 0.05 0.9 0.5 N 0
  + Hit Normal 19b 0.05 0.9 0.5 N 0
  + Hit Normal 20b 0.05 0.9 2 N 0
  + Hit Normal 23b1/4 0.05 0.9 0 N 0
  + Hit Normal 27b1/2 0.05 0.9 0 N 0
  + Hit Normal 32b 0.05 0.9 2 N 0
  + Hit Normal 35b1/2 0.05 0.9 0.5 N 0
  + Hit Normal 40b 0.05 0.9 2 N 0
  + Hit Normal 44b 0.05 0.9 2 N 0
+ Lane 7 0 0 0 0 90 0
  + LaneStep 16b -2 0 Linear Linear 2 0 Linear Linear 1
  + LaneStep 48b -2 0 Linear Linear 2 0 Linear Linear 0.01
  + Hit Normal 18b1/2 0.05 0.9 0.5 N 0
  + Hit Normal 19b1/2 0.05 0.9 0.5 N 0
  + Hit Normal 22b1/2 0.05 0.9 0 N 0
  + Hit Normal 23b1/2 0.05 0.9 0 N 0
  + Hit Normal 26b3/4 0.05 0.9 0.75 N 0
  + Hit Normal 28b 0.05 0.9 2 N 0
  + Hit Normal 34b 0.05 0.9 0.5 N 0
  + Hit Normal 36b 0.05 0.9 2 N 0
  + Hit Normal 39b 0.05 0.9 1 N 0
  + Hit Normal 44b 0.05 0.9 2 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 48b -6 -3 Linear Linear 6 -3 Linear Linear 0.25
  + LaneStep 56b -6 5 Linear Linear 6 5 Linear Linear 0
  + Hit Catch 48b1/4 0.45 0.3 0 N 0
  + Hit Normal 48b3/4 0.1 0.3 0 N 0
  + Hit Normal 49b 0.1 0.3 0 N 0
  + Hit Normal 49b1/4 0.55 0.3 0 N 0
  + Hit Normal 49b1/2 0.1 0.3 0 N 0
  + Hit Normal 50b 0.1 0.3 0 N 0
  + Hit Catch 50b1/4 0.45 0.3 0 N 0
  + Hit Catch 50b3/4 0.45 0.3 0 N 0
  + Hit Normal 51b 0.55 0.3 0 N 0
  + Hit Normal 51b1/4 0.15 0.3 0 N 0
  + Hit Normal 51b3/4 0.15 0.3 0 N 0
  + Hit Normal 52b 0.15 0.3 2 N 0
  + Hit Catch 52b1/4 0.45 0.3 0 N 0
  + Hit Normal 53b 0.55 0.3 0 N 0
  + Hit Normal 53b1/4 0.35 0.3 0 N 0
  + Hit Normal 54b 0.65 0.3 2 N 0
  + Hit Catch 54b1/4 0.45 0.3 0 N 0
  + Hit Normal 54b3/4 0.1 0.3 0 N 0
  + Hit Normal 55b 0.55 0.3 0 N 0
  + Hit Normal 55b1/4 0.25 0.3 0 N 0
  + Hit Normal 55b3/4 0.15 0.3 0 N 0
  + Hit Normal 56b 0.15 0.3 0 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 56b 6 5 Linear Linear -6 5 Linear Linear 0.25
  + LaneStep 64b 6 -3 Linear Linear -6 -3 Linear Linear 0
  + Hit Catch 56b1/4 0.45 0.3 0 N 0
  + Hit Normal 56b3/4 0.1 0.3 0 N 0
  + Hit Normal 57b 0.1 0.3 0 N 0
  + Hit Normal 57b1/4 0.55 0.3 0 N 0
  + Hit Normal 57b1/2 0.1 0.3 0 N 0
  + Hit Normal 58b 0.1 0.3 0 N 0
  + Hit Catch 58b1/4 0.45 0.3 0 N 0
  + Hit Catch 58b3/4 0.45 0.3 0 N 0
  + Hit Normal 59b 0.55 0.3 0 N 0
  + Hit Normal 59b1/4 0.15 0.3 0 N 0
  + Hit Normal 59b3/4 0.15 0.3 0 N 0
  + Hit Normal 60b 0.15 0.3 2 N 0
  + Hit Catch 60b1/4 0.45 0.3 0 N 0
  + Hit Normal 61b 0.55 0.3 0 N 0
  + Hit Normal 61b1/4 0.35 0.3 0 N 0
  + Hit Normal 62b 0.65 0.3 2 N 0
  + Hit Catch 62b1/4 0.45 0.3 0 N 0
  + Hit Normal 62b3/4 0.1 0.3 0 N 0
  + Hit Normal 63b 0.55 0.3 0 N 0
  + Hit Normal 63b1/4 0.25 0.3 0 N 0
  + Hit Normal 63b3/4 0.15 0.3 0 N 0
  + Hit Normal 64b 0.15 0.3 0 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 64b -6 -3 Linear Linear 6 -3 Linear Linear 0.25
  + LaneStep 72b -6 5 Linear Linear 6 5 Linear Linear 0
  + Hit Catch 64b1/4 0.45 0.3 0 N 0
  + Hit Normal 64b3/4 0.1 0.3 0 N 0
  + Hit Normal 65b 0.1 0.3 0 N 0
  + Hit Normal 65b1/4 0.55 0.3 0 N 0
  + Hit Normal 65b1/2 0.1 0.3 0 N 0
  + Hit Normal 66b 0.1 0.3 0 N 0
  + Hit Catch 66b1/4 0.45 0.3 0 N 0
  + Hit Catch 66b3/4 0.45 0.3 0 N 0
  + Hit Normal 67b 0.55 0.3 0 N 0
  + Hit Normal 67b1/4 0.15 0.3 0 N 0
  + Hit Normal 67b3/4 0.15 0.3 0 N 0
  + Hit Normal 68b 0.15 0.3 2 N 0
  + Hit Catch 68b1/4 0.45 0.3 0 N 0
  + Hit Normal 69b 0.55 0.3 0 N 0
  + Hit Normal 69b1/4 0.35 0.3 0 N 0
  + Hit Normal 70b 0.65 0.3 2 N 0
  + Hit Catch 70b1/4 0.45 0.3 0 N 0
  + Hit Normal 70b3/4 0.1 0.3 0 N 0
  + Hit Normal 71b 0.55 0.3 0 N 0
  + Hit Normal 71b1/4 0.25 0.3 0 N 0
  + Hit Normal 71b3/4 0.15 0.3 0 N 0
  + Hit Normal 72b 0.15 0.3 0 N 0
+ Lane 7 1 0 0 0 90 0
  + LaneStep 64b -6 0 Linear Linear 6 0 Linear Linear 0.25
  + LaneStep 80b -6 0 Linear Linear 6 0 Linear Linear 0.01
  + Hit Normal 66b 0.1 0.8 1 N 0
  + Hit Normal 67b1/2 0.1 0.8 0.5 N 0
  + Hit Normal 70b 0.1 0.8 1 N 0
  + Hit Normal 71b1/2 0.1 0.8 0.5 N 0
  + Hit Normal 74b 0.1 0.8 1 N 0
  + Hit Normal 75b1/2 0.1 0.8 0.5 N 0
  + Hit Normal 78b 0.1 0.8 1 N 0
+ Lane -7 1 0 0 0 90 0
  + LaneStep 64b -6 0 Linear Linear 6 0 Linear Linear 0.25
  + LaneStep 80b -6 0 Linear Linear 6 0 Linear Linear 0.01
  + Hit Normal 67b 0.1 0.8 0.5 N 0
  + Hit Normal 71b 0.1 0.8 0.5 N 0
  + Hit Normal 72b 0.1 0.8 2 N 0
  + Hit Normal 76b 0.1 0.8 2 N 0
  + Hit Normal 79b 0.1 0.8 1 N 0
  + Hit Normal 75b 0.1 0.8 0.5 N 0
  + Hit Normal 64b 0.1 0.8 2 N 0
  + Hit Normal 68b 0.1 0.8 2 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 72b 6 5 Linear Linear -6 5 Linear Linear 0.25
  + LaneStep 80b 6 -3 Linear Linear -6 -3 Linear Linear 0
  + Hit Catch 72b1/4 0.45 0.3 0 N 0
  + Hit Normal 72b3/4 0.1 0.3 0 N 0
  + Hit Normal 73b 0.1 0.3 0 N 0
  + Hit Normal 73b1/4 0.55 0.3 0 N 0
  + Hit Normal 73b1/2 0.1 0.3 0 N 0
  + Hit Normal 74b 0.1 0.3 0 N 0
  + Hit Catch 74b1/4 0.45 0.3 0 N 0
  + Hit Catch 74b3/4 0.45 0.3 0 N 0
  + Hit Normal 75b 0.55 0.3 0 N 0
  + Hit Normal 75b1/4 0.15 0.3 0 N 0
  + Hit Normal 75b3/4 0.15 0.3 0 N 0
  + Hit Normal 76b 0.15 0.3 2 N 0
  + Hit Catch 76b1/4 0.45 0.3 0 N 0
  + Hit Normal 77b 0.55 0.3 0 N 0
  + Hit Normal 77b1/4 0.35 0.3 0 N 0
  + Hit Normal 78b 0.65 0.3 2 N 0
  + Hit Catch 78b1/4 0.45 0.3 0 N 0
  + Hit Normal 78b3/4 0.1 0.3 0 N 0
  + Hit Normal 79b 0.55 0.3 0 N 0
  + Hit Normal 79b1/4 0.25 0.3 0 N 0
  + Hit Normal 79b3/4 0.15 0.3 0 N 0
  + Hit Normal 80b 0.15 0.3 0 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 80b 0 -3 Linear Linear 4 -3 Linear Linear 0.25
  + LaneStep 84b 3.5 5 Quadratic/InOut Quadratic/InOut 6.5 3 Quadratic/InOut Quadratic/InOut 0.01
  + LaneStep 96b 3.5 5 Linear Linear 6.5 3 Linear Linear 0.01
  + Hit Catch 80b 0.05 0.9 16 N 0
+ Lane -0.002512038 -0.003516912 0 0 0 0 0
  + LaneStep 84b -3.5 5 Linear Linear -6.5 3 Linear Linear 1
  + LaneStep 88b 0 -3 Quadratic/InOut Quadratic/InOut -4 -3 Quadratic/InOut Quadratic/InOut 0.01
  + LaneStep 96b 4 -3 Quadratic/InOut Quadratic/InOut 0 -3 Quadratic/InOut Sine/InOut 0.01
  + Hit Catch 84b 0.05 0.9 12 N 0
+ Lane -0.002512038 -0.003516912 0 0 0 0 0
  + LaneStep 88b -3.5 5 Linear Linear -6.5 3 Linear Linear 1
  + LaneStep 92b -4 2 Quadratic/InOut Quadratic/InOut -4 -2 Quadratic/InOut Quadratic/InOut 0.01
  + LaneStep 96b -4 2 Linear Linear -4 -2 Linear Linear 0.01
  + Hit Catch 88b 0.05 0.9 8 N 0
+ Lane 0 0 0 0 0 0 0
  $ Offset_X 96b 8 6 _ Quadratic/InOut
  $ OffsetRotation_Z 96b 8 60 _ Quadratic/InOut
  + LaneStep 92b -0.5 4 Linear Linear 2.5 2 Linear Linear 1
  + LaneStep 96b 0 -1 Quadratic/InOut Quadratic/InOut 3 1 Quadratic/InOut Quadratic/InOut 0.01
  + LaneStep 112b 0 -1 Linear Linear 3 1 Linear Linear 0.01
  + Hit Catch 92b 0.05 0.9 20 N 0
+ Lane 0 0 0 0 0 0 0
  $ Offset_X 96b 8 -6 _ Quadratic/InOut
  $ OffsetRotation_Z 96b 8 -60 _ Quadratic/InOut
  + LaneStep 96b 0 -1 Linear Linear -3 1 Linear Linear 1
  + LaneStep 112b 0 -1 Linear Linear -3 1 Linear Linear 0.01
  + Hit Normal 96b 0.05 0.9 16 N 0
+ Lane 0 0 0 0 0 0 0
  $ Offset_X 104b 8 2 _ Quadratic/InOut
  + LaneStep 100b 0 -3 Linear Linear 4 -3 Linear Linear 1
  + LaneStep 112b 0 -3 Linear Linear 4 -3 Linear Linear 0.01
  + Hit Normal 100b 0.05 0.9 12 N 0
+ Lane 0 0 0 0 0 0 0
  $ Offset_X 104b 8 -2 _ Quadratic/InOut
  + LaneStep 104b 0 -3 Linear Linear -4 -3 Linear Linear 1
  + LaneStep 112b 0 -3 Linear Linear -4 -3 Linear Linear 0.01
  + Hit Normal 104b 0.05 0.9 8 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 108b -2 0 Linear Linear 2 0 Linear Linear 1
  + LaneStep 112b -2 0 Linear Linear 2 0 Linear Linear 0.01
  + Hit Normal 108b 0.05 0.9 4 N 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 112b 7 -2 Linear Linear 7 5 Linear Linear 0.25
  + LaneStep 140b 7 -2 Linear Linear 7 5 Linear Linear 0.01
  + Hit Catch 112b 0.05 0.9 2 N 0
  + Hit Normal 114b 0.35 0.6 2 N 0
  + Hit Normal 118b 0.55 0.35 2 N 0
  + Hit Normal 118b 0.15 0.35 1 N 0
  + Hit Normal 120b 0.05 0.35 2 N 0
  + Hit Normal 122b 0.4 0.35 1 N 0
  + Hit Normal 123b 0.15 0.35 1 N 0
  + Hit Normal 124b 0.5 0.35 2 N 0
  + Hit Normal 126b 0.1 0.35 1.5 N 0
  + Hit Normal 127b1/2 0.4 0.35 0.5 N 0
  + Hit Normal 128b 0.05 0.9 2 N 0
  + Hit Normal 130b 0.05 0.6 2 N 0
  + Hit Normal 132b 0.55 0.35 2 N 0
  + Hit Normal 132b 0.15 0.35 2 N 0
  + Hit Normal 135b 0.05 0.35 0.5 N 0
  + Hit Normal 135b1/2 0.35 0.35 0.5 N 0
  + Hit Normal 136b 0.6 0.35 2 N 0
  + Hit Normal 138b 0.25 0.35 2 N 0
+ Lane 0 -3 0 0 0 0 0
  + LaneStep 112b -6 0 Linear Linear 6 0 Linear Linear 0.25
  + LaneStep 140b -6 0 Linear Linear 6 0 Linear Linear 0.05
  + LaneStep 143b -6 0 Linear Linear 6 0 Linear Linear 0.01
  + Hit Normal 112b 0.65 0.25 0 N 0
  + Hit Normal 112b 0.1 0.25 0 N 0
  + Hit Catch 112b1/4 0.5 0.25 0 N 0
  + Hit Catch 112b1/2 0.35 0.25 0 N 0
  + Hit Catch 112b3/4 0.2 0.25 0 N 0
  + Hit Normal 113b1/2 0.35 0.25 0 N 0
  + Hit Normal 113b3/4 0.5 0.25 0 N 0
  + Hit Normal 114b 0.7 0.25 0 N 0
  + Hit Catch 114b 0.2 0.25 0 N 0
  + Hit Normal 114b1/4 0.35 0.25 0 N 0
  + Hit Normal 114b3/4 0.1 0.25 0 N 0
  + Hit Normal 115b 0.25 0.25 0 N 0
  + Hit Normal 115b1/4 0.4 0.25 0 N 0
  + Hit Normal 115b1/2 0.55 0.25 0 N 0
  + Hit Normal 115b3/4 0.7 0.25 0 N 0
  + Hit Normal 116b 0.2 0.25 0 N 0
  + Hit Catch 116b 0.55 0.25 0 N 0
  + Hit Normal 116b1/4 0.35 0.25 0 N 0
  + Hit Normal 117b 0.4 0.25 0 N 0
  + Hit Normal 117b1/4 0.55 0.25 0 N 0
  + Hit Normal 117b1/2 0.1 0.25 0 N 0
  + Hit Normal 117b3/4 0.25 0.25 0 N 0
  + Hit Catch 118b 0.65 0.25 0 N 0
  + Hit Normal 118b1/2 0.4 0.25 0 N 0
  + Hit Normal 118b3/4 0.55 0.25 0 N 0
  + Hit Normal 119b 0.25 0.25 0 N 0
  + Hit Catch 119b 0.7 0.25 0 N 0
  + Hit Normal 119b3/8 0.45 0.25 0 N 0
  + Hit Normal 119b3/4 0.3 0.25 0 N 0
  + Hit Normal 120b 0.6 0.25 0 N 0
  + Hit Catch 120b 0.15 0.25 0 N 0
  + Hit Normal 120b1/4 0.45 0.25 0 N 0
  + Hit Normal 121b 0.05 0.25 0 N 0
  + Hit Normal 121b1/4 0.25 0.25 0 N 0
  + Hit Normal 121b1/2 0.65 0.25 0 N 0
  + Hit Normal 121b3/4 0.45 0.25 0 N 0
  + Hit Catch 122b 0.3 0.25 0 N 0
  + Hit Normal 122b3/4 0.55 0.25 0 N 0
  + Hit Normal 123b 0.25 0.25 0 N 0
  + Hit Catch 123b 0.7 0.25 0 N 0
  + Hit Normal 123b3/8 0.45 0.25 0 N 0
  + Hit Normal 123b3/4 0.3 0.25 0 N 0
  + Hit Catch 124b 0.15 0.25 0 N 0
  + Hit Normal 124b 0.6 0.25 0 N 0
  + Hit Normal 125b1/2 0.05 0.25 0 N 0
  + Hit Normal 125b3/4 0.2 0.25 0 N 0
  + Hit Normal 126b 0.35 0.25 0 N 0
  + Hit Normal 126b1/4 0.5 0.25 0 N 0
  + Hit Normal 126b1/2 0.65 0.25 0 N 0
  + Hit Normal 126b3/4 0.2 0.25 0 N 0
  + Hit Normal 127b 0.55 0.25 0 N 0
  + Hit Normal 127b1/4 0.4 0.25 0 N 0
  + Hit Normal 127b1/2 0.25 0.25 0 N 0
  + Hit Normal 127b3/4 0.4 0.25 0 N 0
  + Hit Normal 128b 0.15 0.25 0 N 0
  + Hit Catch 128b 0.55 0.25 0 N 0
  + Hit Normal 128b1/4 0.3 0.25 0 N 0
  + Hit Normal 128b1/2 0.65 0.25 0 N 0
  + Hit Normal 128b3/4 0.5 0.25 0 N 0
  + Hit Normal 129b1/2 0.2 0.25 0 N 0
  + Hit Normal 129b3/4 0.35 0.25 0 N 0
  + Hit Catch 130b 0.5 0.25 0 N 0
  + Hit Normal 130b 0.2 0.25 0 N 0
  + Hit Normal 130b1/4 0.7 0.25 0 N 0
  + Hit Normal 130b3/4 0.05 0.25 0 N 0
  + Hit Normal 131b 0.2 0.25 0 N 0
  + Hit Normal 131b1/4 0.35 0.25 0 N 0
  + Hit Normal 131b1/2 0.5 0.25 0 N 0
  + Hit Normal 131b3/4 0.65 0.25 0 N 0
  + Hit Normal 132b 0.15 0.25 0 N 0
  + Hit Catch 132b 0.5 0.25 0 N 0
  + Hit Normal 132b1/4 0.3 0.25 0 N 0
  + Hit Normal 133b 0.35 0.25 0 N 0
  + Hit Normal 133b1/4 0.2 0.25 0 N 0
  + Hit Normal 133b1/2 0.7 0.25 0 N 0
  + Hit Normal 133b3/4 0.2 0.25 0 N 0
  + Hit Catch 134b 0.5 0.25 0 N 0
  + Hit Normal 134b3/4 0.55 0.25 0 N 0
  + Hit Normal 135b 0.25 0.25 0 N 0
  + Hit Catch 135b 0.7 0.25 0 N 0
  + Hit Normal 135b3/8 0.45 0.25 0 N 0
  + Hit Normal 135b3/4 0.3 0.25 0 N 0
  + Hit Catch 136b 0.15 0.25 0 N 0
  + Hit Normal 136b 0.6 0.25 0 N 0
  + Hit Normal 136b1/4 0.45 0.25 0 N 0
  + Hit Normal 137b 0.05 0.25 0 N 0
  + Hit Normal 137b1/4 0.25 0.25 0 N 0
  + Hit Normal 137b1/2 0.65 0.25 0 N 0
  + Hit Normal 137b3/4 0.45 0.25 0 N 0
  + Hit Catch 138b 0.3 0.25 0 N 0
  + Hit Normal 138b3/4 0.55 0.25 0 N 0
  + Hit Normal 139b 0.25 0.25 0 N 0
  + Hit Catch 139b 0.7 0.25 0 N 0
  + Hit Normal 139b3/8 0.45 0.25 0 N 0
  + Hit Normal 139b3/4 0.3 0.25 0 N 0
  + Hit Normal 140b 0.15 0.25 3 N 0
  + Hit Normal 140b 0.6 0.25 3 N 0
  + Hit Catch 143b 0.15 0.25 0 F180 0
  + Hit Catch 143b 0.6 0.25 0 F180 0
+ Lane 0 0 0 0 0 0 0
  + LaneStep 112b -7 -2 Linear Linear -7 5 Linear Linear 0.25
  + LaneStep 140b -7 -2 Linear Linear -7 5 Linear Linear 0.01
  + Hit Catch 112b 0.05 0.9 2 N 0
  + Hit Normal 114b 0.05 0.6 2 N 0
  + Hit Normal 116b 0.55 0.35 2 N 0
  + Hit Normal 116b 0.15 0.35 2 N 0
  + Hit Normal 119b 0.05 0.35 0.5 N 0
  + Hit Normal 119b1/2 0.35 0.35 0.5 N 0
  + Hit Normal 120b 0.6 0.35 2 N 0
  + Hit Normal 122b 0.25 0.35 2 N 0
  + Hit Normal 124b 0.5 0.35 2 N 0
  + Hit Normal 126b 0.15 0.35 1 N 0
  + Hit Normal 127b 0.25 0.35 1 N 0
  + Hit Normal 128b 0.05 0.9 2 N 0
  + Hit Normal 130b 0.35 0.6 2 N 0
  + Hit Normal 134b 0.55 0.35 2 N 0
  + Hit Normal 134b 0.15 0.35 1 N 0
  + Hit Normal 136b 0.05 0.35 2 N 0
  + Hit Normal 138b 0.4 0.35 1 N 0
  + Hit Normal 139b 0.15 0.35 1 N 0