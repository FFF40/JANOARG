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
Pivot: 0 0 0
Rotation: 0 0 0
Distance: 10
$ CameraRotation_X 8b 1 10 _ Quadratic/In
$ CameraPivot_Z 8b 1 0.5 _ Quadratic/In
$ CameraPivot_Z 11b 1 1 _ Quadratic/Out
$ CameraRotation_X 11b 1 20 _ Quadratic/Out
$ PivotDistance 76b 4 12 _ Quadratic/Out
$ CameraRotation_X 76b 4 0 _ Quadratic/Out
$ CameraRotation_X 101b1/2 5.5 90 _ Quadratic/In
$ CameraRotation_X 107b1/2 0.5 0 _ Quadratic/Out

[GROUPS]
+ Group 0 0 0 0 0 0
  Name: Group 1
  $ Rotation_Z 12b 1 3240 0 Linear
  $ Rotation_Y 12b 1 -4680 0 Linear
  $ Rotation_X 12b 1 3600 0 Linear
  $ Position_Y 13b 1 0 -0.5 Quadratic/Out
  $ Rotation_Z 16b 1 3240 0 Linear
  $ Rotation_Y 16b 1 -4680 0 Linear
  $ Rotation_X 16b 1 3600 0 Linear
  $ Position_Y 17b 1 0 -0.5 Quadratic/Out
  $ Rotation_Z 20b 1 3240 0 Linear
  $ Rotation_Y 20b 1 -4680 0 Linear
  $ Rotation_X 20b 1 3600 0 Linear
  $ Position_Y 21b 1 0 -0.5 Quadratic/Out
  $ Position_Y 22b 0 -1000 _ Linear
  $ Position_Y 23b990/991 0 0 _ Linear
  $ Rotation_Z 24b 1 3240 0 Linear
  $ Rotation_Y 24b 1 -4680 0 Linear
  $ Rotation_X 24b 1 3600 0 Linear
  $ Position_Y 25b 1 0 -0.5 Quadratic/Out
  $ Rotation_X 28b 1 3600 0 Linear
  $ Rotation_Z 28b 1 3240 0 Linear
  $ Rotation_Y 28b 1 -4680 0 Linear
  $ Position_Y 29b 1 0 -0.5 Quadratic/Out
  $ Rotation_Y 32b 1 -4680 0 Linear
  $ Rotation_X 32b 1 3600 0 Linear
  $ Rotation_Z 32b 1 3240 0 Linear
  $ Position_Y 33b 1 0 -0.5 Quadratic/Out
  $ Rotation_X 36b 1 3600 0 Linear
  $ Rotation_Z 36b 1 3240 0 Linear
  $ Rotation_Y 36b 1 -4680 0 Linear
  $ Position_Y 37b 1 0 -0.5 Quadratic/Out
  $ Rotation_Y 40b 1 -4680 0 Linear
  $ Rotation_Z 40b 1 3240 0 Linear
  $ Rotation_X 40b 1 3600 0 Linear
  $ Position_Y 41b1/9160 0 -1000 _ Linear
  $ Rotation_X 44b 1 3600 0 Linear
  $ Rotation_Y 44b 1 -4680 0 Linear
  $ Position_Y 44b 0 0 _ Linear
  $ Rotation_Z 44b 1 3240 0 Linear
  $ Position_Y 45b 1 0 -0.5 Quadratic/Out
  $ Rotation_Y 48b 1 -4680 0 Linear
  $ Rotation_Z 48b 1 3240 0 Linear
  $ Rotation_X 48b 1 3600 0 Linear
  $ Position_Y 49b 1 0 -0.5 Quadratic/Out
  $ Rotation_Z 52b 1 3240 0 Linear
  $ Rotation_Y 52b 1 -4680 0 Linear
  $ Rotation_X 52b 1 3600 0 Linear
  $ Position_Y 53b 1 0 -0.5 Quadratic/Out
  $ Rotation_Z 56b 1 3240 0 Linear
  $ Rotation_X 56b 1 3600 0 Linear
  $ Rotation_Y 56b 1 -4680 0 Linear
  $ Position_Y 57b 1 0 -0.5 Quadratic/Out
  $ Rotation_Y 60b 1 -4680 0 Linear
  $ Rotation_X 60b 1 3600 0 Linear
  $ Rotation_Z 60b 1 3240 0 Linear
  $ Position_Y 61b 1 0 -0.5 Quadratic/Out
  $ Rotation_Y 64b 1 -4680 0 Linear
  $ Rotation_X 64b 1 3600 0 Linear
  $ Rotation_Z 64b 1 3240 0 Linear
  $ Position_Y 65b 1 0 -0.5 Quadratic/Out
  $ Rotation_X 68b 1 3600 0 Linear
  $ Rotation_Y 68b 1 -4680 0 Linear
  $ Rotation_Z 68b 1 3240 0 Linear
  $ Position_X 69b 0.5 0 -3 Circle/Out
  $ Position_Y 69b 1 0 -0.5 Quadratic/Out
  $ Rotation_Y 72b 1 -4680 0 Linear
  $ Rotation_Z 72b 1 3240 0 Linear
  $ Rotation_X 72b 1 3600 0 Linear
  $ Position_Y 73b 1 0 -0.5 Quadratic/Out

[PALLETE]
Background: 0.995 1 0.721 1
Interface: 0.024 0.058 0 1
$ BackgroundColor_R 76b 2 0.995 1 Linear
$ BackgroundColor_G 76b 2 1 1 Linear
$ BackgroundColor_B 76b 2 0.721 1 Linear
+ LaneStyle 0.049 0.058 0 0.35 0.049 0.058 0 1
+ HitStyle 0.04857114 0.05833333 0 0.35 0.04857114 0.05833333 0 1 0.291 0.363 0 1

[OBJECTS]
+ Lane 0 -1000 0 180 0 0 -1
  Group: Group 1
  $ Offset_Y 8b 0 -4 _ Linear
  $ OffsetRotation_X 9b 0 0 _ Linear
  + LaneStep 9b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 58b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 60b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 80b -8 0 Linear Linear 8 0 Linear Linear 1
  + Hit Normal 9b 0 0.9999999 0 N 0
  + Hit Normal 10b1/2 0.5 0.25 0 N 0
  + Hit Normal 10b5/6 0.7 0.2499998 0 N 0
  + Hit Normal 11b 0.3 0.25 0.25 N 0
  + Hit Normal 11b1/3 0.6 0.25 0 N 0
  + Hit Normal 11b1/2 0.2 0.25 0 N 0
  + Hit Catch 11b21/40 0.1 0.25 0.2875 N 0
  + Hit Normal 11b5/6 0.4 0.25 0 N 0
  + Hit Normal 12b 0 0.25 0 N 0
  + Hit Normal 18b 0.7 0.25 0 N 0
  + Hit Normal 18b1/2 0.5 0.25 0 N 0
  + Hit Normal 19b 0.35 0.25 0 N 0
  + Hit Normal 19b1/2 0.2 0.25 0 N 0
  + Hit Normal 20b 0.05 0.25 0 N 0
  + Hit Catch 46b 0 1 0 N 0
  + Hit Catch 47b 0 1 0 N 0
  + Hit Catch 48b 0 1 0 N 0
  + Hit Normal 50b 0 1 0 N 0
  + Hit Catch 51b 0 1 0 N 0
  + Hit Normal 54b 0 1 0 N 0
  + Hit Catch 55b 0 1 0 N 0
  + Hit Normal 56b 0 1 0 N 0
  + Hit Normal 58b 0 1 0 N 0
  + Hit Catch 62b 0 1 0 N 0
  + Hit Catch 63b 0 1 0 F0 0
  + Hit Catch 64b 0 1 0 N 0
  + Hit Normal 66b 0 1 0 N 0
  + Hit Catch 67b 0 1 0 N 0
  + Hit Normal 68b 0 1 0 F0 0
  + Hit Normal 70b 0 1 0 N 0
  + Hit Catch 71b 0 1 0 F0 0
  + Hit Normal 72b 0 1 0 N 0
  + Hit Normal 73b 0 1 0 N 0
  + Hit Normal 74b 0 1 0 N 0
+ Lane 0 1000 0 0 0 0 -1
  Group: Group 1
  $ OffsetRotation_Z 6b1/2 0 20 _ Linear
  $ Offset_Y 6b1/2 0 3 _ Linear
  $ Offset_Y 7b 2 -4 _ Quartic/In
  $ OffsetRotation_Z 7b 2 0 _ Quadratic/Out
  $ Offset_Y 30b 0.25 -3 _ Quadratic/Out
  $ Offset_Y 30b1/4 0.25 -4 _ Quadratic/In
  $ Offset_Y 30b1/2 0.25 -2 _ Quadratic/Out
  $ Offset_Y 30b3/4 0.25 -4 _ Quadratic/In
  $ Offset_Y 31b 0.25 -1 _ Quadratic/Out
  $ Offset_Y 31b1/4 0.29125 -4 _ Quadratic/In
  $ Offset_Y 31b17/32 0.25 0 _ Quadratic/Out
  $ Offset_Y 31b220/289 0.25 -4 _ Quadratic/In
  $ Offset_Y 46b 0.25 -3 _ Quadratic/Out
  $ Offset_Y 46b1/4 0.25 -4 _ Quadratic/In
  $ Offset_Y 46b1/2 0.25 -2 _ Quadratic/Out
  $ Offset_Y 46b3/4 0.25 -4 _ Quadratic/In
  $ Offset_Y 47b 0.25 -1 _ Quadratic/Out
  $ Offset_Y 47b1/4 0.29125 -4 _ Quadratic/In
  $ Offset_Y 47b17/32 0.25 0 _ Quadratic/Out
  $ Offset_Y 47b220/289 0.25 -4 _ Quadratic/In
  $ Offset_Y 62b 0.25 -3 _ Quadratic/Out
  $ Offset_Y 62b1/4 0.25 -4 _ Quadratic/In
  $ Offset_Y 62b1/2 0.25 -2 _ Quadratic/Out
  $ Offset_X 62b1/2 0.5 4.8 _ Linear
  $ Offset_Y 62b3/4 0.25 -4 _ Quadratic/In
  $ Offset_Y 63b 0.25 -1 _ Quadratic/Out
  $ Offset_Y 63b1/4 0.29125 -4 _ Quadratic/In
  $ Offset_X 63b17/32 0.46875 0 _ Linear
  $ Offset_Y 63b17/32 0.21675 0 _ Quadratic/Out
  $ Offset_Y 63b3/4 0.25 -4 _ Quadratic/In
  + LaneStep 9b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 29b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 30b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 32b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 44b24680/24681 -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 45b24680/24681 -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 47b24680/24681 -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 61b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 62b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 64b -8 0 Linear Linear 8 0 Linear Linear 0
  + Hit Catch 9b 0 0.25 0 N 0
  + Hit Normal 30b 0.25 0.2 0 N 0
  + Hit Normal 30b1/2 0.25 0.2 0 N 0
  + Hit Normal 31b 0.25 0.2 0 N 0
  + Hit Normal 31b17/32 0.25 0.2 0 N 0
  + Hit Normal 32b 0.25 0.2 0 N 0
  + Hit Normal 46b 0.25 0.2 0 N 0
  + Hit Normal 46b1/2 0.25 0.2 0 N 0
  + Hit Normal 47b 0.25 0.2 0 N 0
  + Hit Normal 47b17/32 0.25 0.2 0 N 0
  + Hit Normal 48b 0.25 0.2 0 N 0
  + Hit Normal 62b 0.25 0.2 0 N 0
  + Hit Normal 62b1/2 0.25 0.2 0 N 0
  + Hit Normal 63b 0.25 0.2 0 N 0
  + Hit Normal 63b17/32 0.25 0.2 0 N 0
  + Hit Normal 64b 0.25 0.2 0 N 0
+ Lane 0 1000 0 0 0 0 -1
  Group: Group 1
  $ OffsetRotation_Z 7b 0 -10 _ Linear
  $ Offset_Y 7b 0 0 _ Linear
  $ Offset_Y 7b1/2 1.5 -4 _ Quartic/In
  $ OffsetRotation_Z 7b1/2 1.5 0 _ Quadratic/Out
  $ Offset_Y 30b1/3 0.25 -3 _ Quadratic/Out
  $ Offset_Y 30b7/12 0.25 -4 _ Quadratic/In
  $ Offset_Y 30b5/6 0.25 -2 _ Quadratic/Out
  $ Offset_Y 31b1/12 0.29125 -4 _ Quadratic/In
  $ Offset_Y 31b35/96 0.25 -1 _ Quadratic/Out
  $ Offset_Y 31b59/96 0.25 -4 _ Quadratic/In
  $ Offset_Y 46b1/3 0.25 -3 _ Quadratic/Out
  $ Offset_Y 46b7/12 0.25 -4 _ Quadratic/In
  $ Offset_Y 46b5/6 0.25 -2 _ Quadratic/Out
  $ Offset_Y 47b1/12 0.29125 -4 _ Quadratic/In
  $ Offset_Y 47b35/96 0.25 -1 _ Quadratic/Out
  $ Offset_Y 47b59/96 0.25 -4 _ Quadratic/In
  $ Offset_Y 62b1/3 0.25 -3 _ Quadratic/Out
  $ Offset_X 62b1/3 0.5 -4.8 _ Linear
  $ Offset_Y 62b7/12 0.25 -4 _ Quadratic/In
  $ Offset_Y 62b5/6 0.25 -2 _ Quadratic/Out
  $ Offset_Y 63b1/12 0.29125 -4 _ Quadratic/In
  $ Offset_X 63b35/96 0.5 0 _ Linear
  $ Offset_Y 63b35/96 0.25 -1 _ Quadratic/Out
  $ Offset_Y 63b59/96 0.25 -4 _ Quadratic/In
  + LaneStep 9b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 29b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 30b1/3 -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 32b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 45b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 46b3253/9760 -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 48b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 61b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 62b1/3 -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 64b -8 0 Linear Linear 8 0 Linear Linear 0
  + Hit Catch 9b 0.25 0.25 0 N 0
  + Hit Normal 30b1/3 0.55 0.2 0 N 0
  + Hit Normal 30b5/6 0.55 0.2 0 N 0
  + Hit Normal 31b35/96 0.55 0.2 0 N 0
  + Hit Normal 31b83/96 0.55 0.2 0 N 0
  + Hit Normal 46b1/3 0.55 0.2 0 N 0
  + Hit Normal 46b5/6 0.55 0.2 0 N 0
  + Hit Normal 47b35/96 0.55 0.2 0 N 0
  + Hit Normal 47b83/96 0.55 0.2 0 N 0
  + Hit Normal 62b1/3 0.55 0.2 0 N 0
  + Hit Normal 62b5/6 0.55 0.2 0 N 0
  + Hit Normal 63b35/96 0.55 0.2 0 N 0
  + Hit Normal 63b83/96 0.55 0.2 0 N 0
+ Lane 0 1000 0 0 0 0 -1
  Group: Group 1
  $ OffsetRotation_Z 7b1/2 0 10 _ Linear
  $ Offset_Y 7b1/2 0 -1 _ Linear
  $ Offset_Y 8b 1 -4 _ Quartic/In
  $ OffsetRotation_Z 8b 1 0 _ Quadratic/Out
  $ Offset_X 33b1/16 0 0.8 _ Linear
  $ Offset_X 34b1/16 0 4 _ Linear
  $ Offset_X 34b17/32 0 7.6 _ Linear
  $ Offset_X 35b1/16 0 4.4 _ Linear
  $ Offset_X 35b3/8 0 8.4 _ Linear
  $ Offset_X 35b5/12 0 6 _ Linear
  $ Offset_X 35b37/81 0 2.8 _ Linear
  $ Offset_X 35b1/2 0 -0.4 _ Linear
  $ Offset_X 48b 0 0 _ Linear
  $ Offset_X 49b1/16 0 0.8 _ Linear
  $ Offset_X 50b1/16 0 4 _ Linear
  $ Offset_X 50b17/32 0 7.6 _ Linear
  $ Offset_X 51b1/16 0 4.4 _ Linear
  $ Offset_X 51b3/8 0 8.4 _ Linear
  $ Offset_X 51b5/12 0 6 _ Linear
  $ Offset_X 51b37/81 0 2.8 _ Linear
  $ Offset_X 51b1/2 0 -0.4 _ Linear
  $ Offset_X 64b 0 0 _ Linear
  $ OffsetRotation_X 65b 3 -3600 _ Exponential/In
  $ Offset_X 65b1/16 0 0.8 _ Linear
  $ Offset_X 66b1/16 0 4 _ Linear
  $ Offset_X 66b17/32 0 7.6 _ Linear
  $ Offset_X 67b1/16 0 4.4 _ Linear
  $ Offset_X 67b3/8 0 8.4 _ Linear
  $ Offset_X 67b5/12 0 6 _ Linear
  $ Offset_X 67b37/81 0 2.8 _ Linear
  $ Offset_X 67b1/2 0 -0.4 _ Linear
  + LaneStep 9b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 32b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 33b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 36b -8 0 Linear Linear 8 0 Linear Linear 100
  + LaneStep 48b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 49b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 52b -8 0 Linear Linear 8 0 Linear Linear 100
  + LaneStep 64b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 65b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 68b -8 0 Linear Linear 8 0 Linear Linear 100
  + Hit Catch 9b 0.5 0.25 0 N 0
  + Hit Catch 33b 0.2 0.25 0.75 N 0
  + Hit Catch 34b 0.35 0.25 0.4375 N 0
  + Hit Catch 34b1/2 -0.05 0.253 0.40625 N 0
  + Hit Catch 35b 0.125 0.25 0.875 N 0
  + Hit Catch 49b 0.2 0.25 0.75 N 0
  + Hit Catch 50b 0.35 0.25 0.4375 N 0
  + Hit Catch 50b1/2 -0.05 0.253 0.40625 N 0
  + Hit Catch 51b 0.125 0.25 0.875 N 0
  + Hit Catch 65b 0.2 0.25 0.75 N 0
  + Hit Catch 66b 0.35 0.25 0.4375 N 0
  + Hit Catch 66b1/2 -0.05 0.253 0.40625 N 0
  + Hit Catch 67b 0.125 0.25 0.875 N 0
+ Lane 0 1000 0 0 0 0 -1
  Group: Group 1
  $ OffsetRotation_Z 6b 0 -10 _ Linear
  $ Offset_Y 6b 0 5 _ Linear
  $ Offset_Y 6b1/2 2.5 -4 _ Quartic/In
  $ OffsetRotation_Z 6b1/2 2.5 0 _ Quadratic/Out
  + LaneStep 9b -8 0 Linear Linear 8 0 Linear Linear 0
  + Hit Catch 9b 0.75 0.25 0 N 0
+ Lane 0 -1000 0 180 0 0 -1
  Group: Group 1
  $ Offset_Y 8b 0 -4 _ Linear
  $ OffsetRotation_X 9b 0 0 _ Linear
  $ OffsetRotation_X 25b 0 -70 _ Linear
  $ OffsetRotation_X 28b 0 0 _ Linear
  $ Offset_Y 57b 0 -1000 _ Linear
  $ Offset_Y 58b 0 -4 _ Linear
  $ Offset_Y 73b 0 -1000 _ Linear
  $ Offset_Y 74b 0 -4 _ Linear
  + LaneStep 9b -8 0 Linear Linear 8 0 Linear Linear 0.25
  + LaneStep 12b -8 0 Linear Linear 8 0 Linear Linear 0.25
  + LaneStep 13b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 16b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 17b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 20b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 21b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 24b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 25b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 28b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 29b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 32b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 33b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 36b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 37b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 40b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 41b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 44b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 45b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 48b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 49b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 52b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 53b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 56b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 57b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 60b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 61b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 64b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 65b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 68b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 69b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 72b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 73b -8 0 Linear Linear 8 0 Linear Linear 4
  + LaneStep 76b -8 0 Linear Linear 8 0 Linear Linear 0.05
  + LaneStep 86b -8 0 Linear Linear 8 0 Linear Linear 0.25
  + Hit Normal 14b1/2 0.15 0.25 0.2166667 N 0
  + Hit Normal 14b5/6 0.65 0.25 0 N 0
  + Hit Normal 15b 0.4 0.25 0.2166667 N 0
  + Hit Normal 15b1/3 0.2 0.25 0 N 0
  + Hit Normal 15b1/2 0.6 0.25 0.3333333 N 0
  + Hit Normal 15b5/6 0.2 0.25 0 N 0
  + Hit Normal 16b 0.6 0.25 0 N 0
  + Hit Normal 18b1/6 0.35 0.25 0.566667 N 0
  + Hit Normal 19b 0.05 0.25 0.25 N 0
  + Hit Normal 19b1/2 0.5 0.25 0.25 N 0
  + Hit Catch 21b 0.05 0.25 0.5 N 0
  + Hit Normal 26b1/2 0.5 0.25 0 N 0
  + Hit Normal 26b5/6 0.65 0.25 0 N 0
  + Hit Normal 27b 0.35 0.3 0.333334 N 0
  + Hit Normal 27b1/3 0.05 0.25 0 N 0
  + Hit Normal 27b1/2 0.35 0.3 0.1666667 N 0
  + Hit Normal 27b5/6 0.35 0.3 0 N 0
  + Hit Catch 30b1/32 0.25 0.2 0 N 0
  + Hit Catch 33b 0.2 0.25 0 N 0
  + Hit Catch 33b1/16 0.25 0.25 0 N 0
  + Hit Catch 34b 0.4 0.25 0 N 0
  + Hit Catch 34b1/16 0.6 0.25 0 N 0
  + Hit Catch 34b1/2 0.2 0.25 0 N 0
  + Hit Catch 34b17/32 0.43 0.25 0 N 0
  + Hit Catch 35b 0.6 0.25 0 N 0
  + Hit Catch 35b1/16 0.4 0.25 0 N 0
  + Hit Catch 35b3/8 0.65 0.25 0 N 0
  + Hit Catch 35b5/12 0.5 0.25 0 N 0
  + Hit Catch 35b37/81 0.3 0.25 0 N 0
  + Hit Catch 35b1/2 0.1 0.25 0 N 0
  + Hit Normal 50b 0.4 0.25 0 N 0
  + Hit Catch 50b97/1553 0.6 0.25 0 N 0
  + Hit Normal 50b1/2 0.2 0.25 0 N 0
  + Hit Catch 50b417/785 0.43 0.25 0 N 0
  + Hit Normal 51b 0.6 0.25 0 N 0
  + Hit Catch 51b97/1553 0.4 0.25 0 N 0
  + Hit Catch 51b1159/3091 0.65 0.25 0 N 0
  + Hit Catch 51b982/2357 0.5 0.25 0 N 0
  + Hit Catch 51b169/370 0.3 0.25 0 N 0
  + Hit Catch 51b1/2 0.1 0.25 0 N 0
  + Hit Normal 58b1/2 0.5 0.25 0 N 0
  + Hit Normal 58b5/6 0.65 0.25 0 N 0
  + Hit Normal 59b 0.35 0.3 0.333334 N 0
  + Hit Normal 59b1/3 0.05 0.25 0 N 0
  + Hit Normal 59b1/2 0.35 0.3 0.1666667 N 0
  + Hit Normal 59b5/6 0.35 0.3 0 N 0
  + Hit Normal 60b 0.05 0.25 0 N 0
  + Hit Catch 65b 0.2 0.25 0 N 0
  + Hit Catch 65b1/16 0.25 0.25 0 N 0
  + Hit Normal 66b 0.4 0.25 0 N 0
  + Hit Catch 66b1/16 0.6 0.25 0 N 0
  + Hit Normal 66b1/2 0.2 0.25 0 N 0
  + Hit Catch 66b17/32 0.43 0.25 0 N 0
  + Hit Normal 67b 0.6 0.25 0 N 0
  + Hit Catch 67b1/16 0.4 0.25 0 N 0
  + Hit Catch 67b3/8 0.65 0.25 0 N 0
  + Hit Catch 67b5/12 0.5 0.25 0 N 0
  + Hit Catch 67b37/81 0.3 0.25 0 N 0
  + Hit Catch 67b1/2 0.1 0.25 0 N 0
  + Hit Normal 74b1/2 0.125 0.25 0 N 0
  + Hit Normal 74b5/6 0.35 0.25 0 N 0
  + Hit Normal 75b 0.7 0.25 0.333334 N 0
  + Hit Normal 75b1/3 0.05 0.25 0 N 0
  + Hit Normal 75b1/2 0.4 0.2500001 0.1666667 N 0
  + Hit Normal 75b5/6 0.65 0.25 0 N 0
  + Hit Normal 76b 0.05 0.25 0 N 0
  + Hit Catch 76b 0.35 0.25 0 N 0
  + Hit Catch 76b 0.65 0.25 0 N 0
+ Lane 0 1000 0 0 0 0 0
  Group: Group 1
  $ Offset_Y 9b 0 -4 _ Linear
  $ OffsetRotation_X 30b 2 90 _ Quadratic/Out
  $ OffsetRotation_X 32b 0 0 _ Linear
  $ Offset_X 69b 1 0 2 Circle/Out
  + LaneStep 9b -7.999998 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 76b -7.999999 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 91b -7.999999 0 Linear Linear 8 0 Linear Linear 5
    $ Speed 76b 1 1 0.1 Linear
    $ Speed 77b 1 1 0.1 Linear
    $ Speed 78b 1 1 0.1 Linear
    $ Speed 79b 1 1 0.1 Linear
    $ Speed 80b 1 1 0.1 Linear
    $ Speed 81b 1 1 0.1 Linear
    $ Speed 82b 1 1 0.1 Linear
    $ Speed 83b 1 1 0.1 Linear
    $ Speed 84b 1 1 0.1 Linear
    $ Speed 85b 1 1 0.1 Linear
    $ Speed 86b 1 1 0.1 Linear
    $ Speed 87b 1 1 0.1 Linear
    $ Speed 88b 1 1 0.1 Linear
    $ Speed 89b 1 1 0.1 Linear
    $ Speed 90b 1 1 0.1 Linear
  + LaneStep 92b -7.999999 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 107b -8 0 Linear Linear 8 0 Linear Linear 1
    $ Speed 76b 1 1 0.1 Linear
    $ Speed 77b 1 1 0.1 Linear
    $ Speed 78b 1 1 0.1 Linear
    $ Speed 79b 1 1 0.1 Linear
    $ Speed 80b 1 1 0.1 Linear
    $ Speed 81b 1 1 0.1 Linear
    $ Speed 82b 1 1 0.1 Linear
    $ Speed 83b 1 1 0.1 Linear
    $ Speed 84b 1 1 0.1 Linear
    $ Speed 85b 1 1 0.1 Linear
    $ Speed 86b 1 1 0.1 Linear
    $ Speed 87b 1 1 0.1 Linear
    $ Speed 88b 1 1 0.1 Linear
    $ Speed 89b 1 1 0.1 Linear
    $ Speed 90b 1 1 0.1 Linear
    $ Speed 92b 1 1 0.1 Linear
    $ Speed 93b 1 1 0.1 Linear
    $ Speed 94b 1 1 0.1 Linear
    $ Speed 95b 1 1 0.1 Linear
    $ Speed 96b 1 1 0.1 Linear
    $ Speed 97b 1 1 0.1 Linear
    $ Speed 98b 1 1 0.1 Linear
    $ Speed 99b 1 1 0.1 Linear
    $ Speed 100b 1 1 0.1 Linear
    $ Speed 101b 1 1 0.1 Linear
    $ Speed 102b 1 1 0.1 Linear
    $ Speed 103b 1 1 0.1 Linear
    $ Speed 104b 1 1 0.1 Linear
    $ Speed 105b 1 1 0.1 Linear
    $ Speed 106b 1 1 0.1 Linear
  + Hit Normal 12b -2 5 1 N 0
  + Hit Catch 12b1/64 -2 5 0 N 0
  + Hit Catch 12b1/32 -2 5 0 N 0
  + Hit Catch 12b3/64 -2 5 0 N 0
  + Hit Catch 12b1/16 -2 5 0 N 0
  + Hit Catch 12b5/64 -2 5 0 N 0
  + Hit Catch 12b3/32 -2 5 0 N 0
  + Hit Catch 12b7/64 -2 5 0 N 0
  + Hit Catch 12b1/8 -2 5 0 N 0
  + Hit Catch 12b9/64 -2 5 0 N 0
  + Hit Catch 12b5/32 -2 5 0 N 0
  + Hit Catch 12b11/64 -2 5 0 N 0
  + Hit Catch 12b3/16 -2 5 0 N 0
  + Hit Catch 12b13/64 -2 5 0 N 0
  + Hit Catch 12b7/32 -2 5 0 N 0
  + Hit Catch 12b15/64 -2 5 0 N 0
  + Hit Catch 12b1/4 -2 5 0 N 0
  + Hit Catch 12b17/64 -2 5 0 N 0
  + Hit Catch 12b9/32 -2 5 0 N 0
  + Hit Catch 12b19/64 -2 5 0 N 0
  + Hit Catch 12b5/16 -2 5 0 N 0
  + Hit Catch 12b21/64 -2 5 0 N 0
  + Hit Catch 12b11/32 -2 5 0 N 0
  + Hit Catch 12b23/64 -2 5 0 N 0
  + Hit Catch 12b3/8 -2 5 0 N 0
  + Hit Catch 12b25/64 -2 5 0 N 0
  + Hit Catch 12b13/32 -2 5 0 N 0
  + Hit Catch 12b27/64 -2 5 0 N 0
  + Hit Catch 12b7/16 -2 5 0 N 0
  + Hit Catch 12b29/64 -2 5 0 N 0
  + Hit Catch 12b15/32 -2 5 0 N 0
  + Hit Catch 12b31/64 -2 5 0 N 0
  + Hit Catch 12b1/2 -2 5 0 N 0
  + Hit Catch 12b33/64 -2 5 0 N 0
  + Hit Catch 12b17/32 -2 5 0 N 0
  + Hit Catch 12b35/64 -2 5 0 N 0
  + Hit Catch 12b9/16 -2 5 0 N 0
  + Hit Catch 12b37/64 -2 5 0 N 0
  + Hit Catch 12b19/32 -2 5 0 N 0
  + Hit Catch 12b39/64 -2 5 0 N 0
  + Hit Catch 12b5/8 -2 5 0 N 0
  + Hit Catch 12b41/64 -2 5 0 N 0
  + Hit Catch 12b21/32 -2 5 0 N 0
  + Hit Catch 12b43/64 -2 5 0 N 0
  + Hit Catch 12b11/16 -2 5 0 N 0
  + Hit Catch 12b45/64 -2 5 0 N 0
  + Hit Catch 12b23/32 -2 5 0 N 0
  + Hit Catch 12b47/64 -2 5 0 N 0
  + Hit Catch 12b3/4 -2 5 0 N 0
  + Hit Catch 12b49/64 -2 5 0 N 0
  + Hit Catch 12b25/32 -2 5 0 N 0
  + Hit Catch 12b51/64 -2 5 0 N 0
  + Hit Catch 12b13/16 -2 5 0 N 0
  + Hit Catch 12b53/64 -2 5 0 N 0
  + Hit Catch 12b27/32 -2 5 0 N 0
  + Hit Catch 12b55/64 -2 5 0 N 0
  + Hit Catch 12b7/8 -2 5 0 N 0
  + Hit Catch 12b57/64 -2 5 0 N 0
  + Hit Catch 12b29/32 -2 5 0 N 0
  + Hit Catch 12b59/64 -2 5 0 N 0
  + Hit Catch 12b15/16 -2 5 0 N 0
  + Hit Catch 12b61/64 -2 5 0 N 0
  + Hit Catch 12b31/32 -2 5 0 N 0
  + Hit Catch 12b63/64 -2 5 0 N 0
  + Hit Catch 13b -2 5 0 N 0
  + Hit Normal 16b -2 5 1 N 0
  + Hit Catch 16b1/64 -2 5 0 N 0
  + Hit Catch 16b1/32 -2 5 0 N 0
  + Hit Catch 16b3/64 -2 5 0 N 0
  + Hit Catch 16b1/16 -2 5 0 N 0
  + Hit Catch 16b5/64 -2 5 0 N 0
  + Hit Catch 16b3/32 -2 5 0 N 0
  + Hit Catch 16b7/64 -2 5 0 N 0
  + Hit Catch 16b1/8 -2 5 0 N 0
  + Hit Catch 16b9/64 -2 5 0 N 0
  + Hit Catch 16b5/32 -2 5 0 N 0
  + Hit Catch 16b11/64 -2 5 0 N 0
  + Hit Catch 16b3/16 -2 5 0 N 0
  + Hit Catch 16b13/64 -2 5 0 N 0
  + Hit Catch 16b7/32 -2 5 0 N 0
  + Hit Catch 16b15/64 -2 5 0 N 0
  + Hit Catch 16b1/4 -2 5 0 N 0
  + Hit Catch 16b17/64 -2 5 0 N 0
  + Hit Catch 16b9/32 -2 5 0 N 0
  + Hit Catch 16b19/64 -2 5 0 N 0
  + Hit Catch 16b5/16 -2 5 0 N 0
  + Hit Catch 16b21/64 -2 5 0 N 0
  + Hit Catch 16b11/32 -2 5 0 N 0
  + Hit Catch 16b23/64 -2 5 0 N 0
  + Hit Catch 16b3/8 -2 5 0 N 0
  + Hit Catch 16b25/64 -2 5 0 N 0
  + Hit Catch 16b13/32 -2 5 0 N 0
  + Hit Catch 16b27/64 -2 5 0 N 0
  + Hit Catch 16b7/16 -2 5 0 N 0
  + Hit Catch 16b29/64 -2 5 0 N 0
  + Hit Catch 16b15/32 -2 5 0 N 0
  + Hit Catch 16b31/64 -2 5 0 N 0
  + Hit Catch 16b1/2 -2 5 0 N 0
  + Hit Catch 16b33/64 -2 5 0 N 0
  + Hit Catch 16b17/32 -2 5 0 N 0
  + Hit Catch 16b35/64 -2 5 0 N 0
  + Hit Catch 16b9/16 -2 5 0 N 0
  + Hit Catch 16b37/64 -2 5 0 N 0
  + Hit Catch 16b19/32 -2 5 0 N 0
  + Hit Catch 16b39/64 -2 5 0 N 0
  + Hit Catch 16b5/8 -2 5 0 N 0
  + Hit Catch 16b41/64 -2 5 0 N 0
  + Hit Catch 16b21/32 -2 5 0 N 0
  + Hit Catch 16b43/64 -2 5 0 N 0
  + Hit Catch 16b11/16 -2 5 0 N 0
  + Hit Catch 16b45/64 -2 5 0 N 0
  + Hit Catch 16b23/32 -2 5 0 N 0
  + Hit Catch 16b47/64 -2 5 0 N 0
  + Hit Catch 16b3/4 -2 5 0 N 0
  + Hit Catch 16b49/64 -2 5 0 N 0
  + Hit Catch 16b25/32 -2 5 0 N 0
  + Hit Catch 16b51/64 -2 5 0 N 0
  + Hit Catch 16b13/16 -2 5 0 N 0
  + Hit Catch 16b53/64 -2 5 0 N 0
  + Hit Catch 16b27/32 -2 5 0 N 0
  + Hit Catch 16b55/64 -2 5 0 N 0
  + Hit Catch 16b7/8 -2 5 0 N 0
  + Hit Catch 16b57/64 -2 5 0 N 0
  + Hit Catch 16b29/32 -2 5 0 N 0
  + Hit Catch 16b59/64 -2 5 0 N 0
  + Hit Catch 16b15/16 -2 5 0 N 0
  + Hit Catch 16b61/64 -2 5 0 N 0
  + Hit Catch 16b31/32 -2 5 0 N 0
  + Hit Catch 16b63/64 -2 5 0 N 0
  + Hit Catch 17b -2 5 0 N 0
  + Hit Normal 20b -2 5 1 N 0
  + Hit Catch 20b1/64 -2 5 0 N 0
  + Hit Catch 20b1/32 -2 5 0 N 0
  + Hit Catch 20b3/64 -2 5 0 N 0
  + Hit Catch 20b1/16 -2 5 0 N 0
  + Hit Catch 20b5/64 -2 5 0 N 0
  + Hit Catch 20b3/32 -2 5 0 N 0
  + Hit Catch 20b7/64 -2 5 0 N 0
  + Hit Catch 20b1/8 -2 5 0 N 0
  + Hit Catch 20b9/64 -2 5 0 N 0
  + Hit Catch 20b5/32 -2 5 0 N 0
  + Hit Catch 20b11/64 -2 5 0 N 0
  + Hit Catch 20b3/16 -2 5 0 N 0
  + Hit Catch 20b13/64 -2 5 0 N 0
  + Hit Catch 20b7/32 -2 5 0 N 0
  + Hit Catch 20b15/64 -2 5 0 N 0
  + Hit Catch 20b1/4 -2 5 0 N 0
  + Hit Catch 20b17/64 -2 5 0 N 0
  + Hit Catch 20b9/32 -2 5 0 N 0
  + Hit Catch 20b19/64 -2 5 0 N 0
  + Hit Catch 20b5/16 -2 5 0 N 0
  + Hit Catch 20b21/64 -2 5 0 N 0
  + Hit Catch 20b11/32 -2 5 0 N 0
  + Hit Catch 20b23/64 -2 5 0 N 0
  + Hit Catch 20b3/8 -2 5 0 N 0
  + Hit Catch 20b25/64 -2 5 0 N 0
  + Hit Catch 20b13/32 -2 5 0 N 0
  + Hit Catch 20b27/64 -2 5 0 N 0
  + Hit Catch 20b7/16 -2 5 0 N 0
  + Hit Catch 20b29/64 -2 5 0 N 0
  + Hit Catch 20b15/32 -2 5 0 N 0
  + Hit Catch 20b31/64 -2 5 0 N 0
  + Hit Catch 20b1/2 -2 5 0 N 0
  + Hit Catch 20b33/64 -2 5 0 N 0
  + Hit Catch 20b17/32 -2 5 0 N 0
  + Hit Catch 20b35/64 -2 5 0 N 0
  + Hit Catch 20b9/16 -2 5 0 N 0
  + Hit Catch 20b37/64 -2 5 0 N 0
  + Hit Catch 20b19/32 -2 5 0 N 0
  + Hit Catch 20b39/64 -2 5 0 N 0
  + Hit Catch 20b5/8 -2 5 0 N 0
  + Hit Catch 20b41/64 -2 5 0 N 0
  + Hit Catch 20b21/32 -2 5 0 N 0
  + Hit Catch 20b43/64 -2 5 0 N 0
  + Hit Catch 20b11/16 -2 5 0 N 0
  + Hit Catch 20b45/64 -2 5 0 N 0
  + Hit Catch 20b23/32 -2 5 0 N 0
  + Hit Catch 20b47/64 -2 5 0 N 0
  + Hit Catch 20b3/4 -2 5 0 N 0
  + Hit Catch 20b49/64 -2 5 0 N 0
  + Hit Catch 20b25/32 -2 5 0 N 0
  + Hit Catch 20b51/64 -2 5 0 N 0
  + Hit Catch 20b13/16 -2 5 0 N 0
  + Hit Catch 20b53/64 -2 5 0 N 0
  + Hit Catch 20b27/32 -2 5 0 N 0
  + Hit Catch 20b55/64 -2 5 0 N 0
  + Hit Catch 20b7/8 -2 5 0 N 0
  + Hit Catch 20b57/64 -2 5 0 N 0
  + Hit Catch 20b29/32 -2 5 0 N 0
  + Hit Catch 20b59/64 -2 5 0 N 0
  + Hit Catch 20b15/16 -2 5 0 N 0
  + Hit Catch 20b61/64 -2 5 0 N 0
  + Hit Catch 20b31/32 -2 5 0 N 0
  + Hit Catch 20b63/64 -2 5 0 N 0
  + Hit Catch 21b -2 5 0 N 0
  + Hit Normal 24b -2 5 1 N 0
  + Hit Catch 24b1/64 -2 5 0 N 0
  + Hit Catch 24b1/32 -2 5 0 N 0
  + Hit Catch 24b3/64 -2 5 0 N 0
  + Hit Catch 24b1/16 -2 5 0 N 0
  + Hit Catch 24b5/64 -2 5 0 N 0
  + Hit Catch 24b3/32 -2 5 0 N 0
  + Hit Catch 24b7/64 -2 5 0 N 0
  + Hit Catch 24b1/8 -2 5 0 N 0
  + Hit Catch 24b9/64 -2 5 0 N 0
  + Hit Catch 24b5/32 -2 5 0 N 0
  + Hit Catch 24b11/64 -2 5 0 N 0
  + Hit Catch 24b3/16 -2 5 0 N 0
  + Hit Catch 24b13/64 -2 5 0 N 0
  + Hit Catch 24b7/32 -2 5 0 N 0
  + Hit Catch 24b15/64 -2 5 0 N 0
  + Hit Catch 24b1/4 -2 5 0 N 0
  + Hit Catch 24b17/64 -2 5 0 N 0
  + Hit Catch 24b9/32 -2 5 0 N 0
  + Hit Catch 24b19/64 -2 5 0 N 0
  + Hit Catch 24b5/16 -2 5 0 N 0
  + Hit Catch 24b21/64 -2 5 0 N 0
  + Hit Catch 24b11/32 -2 5 0 N 0
  + Hit Catch 24b23/64 -2 5 0 N 0
  + Hit Catch 24b3/8 -2 5 0 N 0
  + Hit Catch 24b25/64 -2 5 0 N 0
  + Hit Catch 24b13/32 -2 5 0 N 0
  + Hit Catch 24b27/64 -2 5 0 N 0
  + Hit Catch 24b7/16 -2 5 0 N 0
  + Hit Catch 24b29/64 -2 5 0 N 0
  + Hit Catch 24b15/32 -2 5 0 N 0
  + Hit Catch 24b31/64 -2 5 0 N 0
  + Hit Catch 24b1/2 -2 5 0 N 0
  + Hit Catch 24b33/64 -2 5 0 N 0
  + Hit Catch 24b17/32 -2 5 0 N 0
  + Hit Catch 24b35/64 -2 5 0 N 0
  + Hit Catch 24b9/16 -2 5 0 N 0
  + Hit Catch 24b37/64 -2 5 0 N 0
  + Hit Catch 24b19/32 -2 5 0 N 0
  + Hit Catch 24b39/64 -2 5 0 N 0
  + Hit Catch 24b5/8 -2 5 0 N 0
  + Hit Catch 24b41/64 -2 5 0 N 0
  + Hit Catch 24b21/32 -2 5 0 N 0
  + Hit Catch 24b43/64 -2 5 0 N 0
  + Hit Catch 24b11/16 -2 5 0 N 0
  + Hit Catch 24b45/64 -2 5 0 N 0
  + Hit Catch 24b23/32 -2 5 0 N 0
  + Hit Catch 24b47/64 -2 5 0 N 0
  + Hit Catch 24b3/4 -2 5 0 N 0
  + Hit Catch 24b49/64 -2 5 0 N 0
  + Hit Catch 24b25/32 -2 5 0 N 0
  + Hit Catch 24b51/64 -2 5 0 N 0
  + Hit Catch 24b13/16 -2 5 0 N 0
  + Hit Catch 24b53/64 -2 5 0 N 0
  + Hit Catch 24b27/32 -2 5 0 N 0
  + Hit Catch 24b55/64 -2 5 0 N 0
  + Hit Catch 24b7/8 -2 5 0 N 0
  + Hit Catch 24b57/64 -2 5 0 N 0
  + Hit Catch 24b29/32 -2 5 0 N 0
  + Hit Catch 24b59/64 -2 5 0 N 0
  + Hit Catch 24b15/16 -2 5 0 N 0
  + Hit Catch 24b61/64 -2 5 0 N 0
  + Hit Catch 24b31/32 -2 5 0 N 0
  + Hit Catch 24b63/64 -2 5 0 N 0
  + Hit Catch 25b -2 5 0 N 0
  + Hit Normal 28b -2 5 1 N 0
  + Hit Catch 28b1/64 -2 5 0 N 0
  + Hit Catch 28b1/32 -2 5 0 N 0
  + Hit Catch 28b3/64 -2 5 0 N 0
  + Hit Catch 28b1/16 -2 5 0 N 0
  + Hit Catch 28b5/64 -2 5 0 N 0
  + Hit Catch 28b3/32 -2 5 0 N 0
  + Hit Catch 28b7/64 -2 5 0 N 0
  + Hit Catch 28b1/8 -2 5 0 N 0
  + Hit Catch 28b9/64 -2 5 0 N 0
  + Hit Catch 28b5/32 -2 5 0 N 0
  + Hit Catch 28b11/64 -2 5 0 N 0
  + Hit Catch 28b3/16 -2 5 0 N 0
  + Hit Catch 28b13/64 -2 5 0 N 0
  + Hit Catch 28b7/32 -2 5 0 N 0
  + Hit Catch 28b15/64 -2 5 0 N 0
  + Hit Catch 28b1/4 -2 5 0 N 0
  + Hit Catch 28b17/64 -2 5 0 N 0
  + Hit Catch 28b9/32 -2 5 0 N 0
  + Hit Catch 28b19/64 -2 5 0 N 0
  + Hit Catch 28b5/16 -2 5 0 N 0
  + Hit Catch 28b21/64 -2 5 0 N 0
  + Hit Catch 28b11/32 -2 5 0 N 0
  + Hit Catch 28b23/64 -2 5 0 N 0
  + Hit Catch 28b3/8 -2 5 0 N 0
  + Hit Catch 28b25/64 -2 5 0 N 0
  + Hit Catch 28b13/32 -2 5 0 N 0
  + Hit Catch 28b27/64 -2 5 0 N 0
  + Hit Catch 28b7/16 -2 5 0 N 0
  + Hit Catch 28b29/64 -2 5 0 N 0
  + Hit Catch 28b15/32 -2 5 0 N 0
  + Hit Catch 28b31/64 -2 5 0 N 0
  + Hit Catch 28b1/2 -2 5 0 N 0
  + Hit Catch 28b33/64 -2 5 0 N 0
  + Hit Catch 28b17/32 -2 5 0 N 0
  + Hit Catch 28b35/64 -2 5 0 N 0
  + Hit Catch 28b9/16 -2 5 0 N 0
  + Hit Catch 28b37/64 -2 5 0 N 0
  + Hit Catch 28b19/32 -2 5 0 N 0
  + Hit Catch 28b39/64 -2 5 0 N 0
  + Hit Catch 28b5/8 -2 5 0 N 0
  + Hit Catch 28b41/64 -2 5 0 N 0
  + Hit Catch 28b21/32 -2 5 0 N 0
  + Hit Catch 28b43/64 -2 5 0 N 0
  + Hit Catch 28b11/16 -2 5 0 N 0
  + Hit Catch 28b45/64 -2 5 0 N 0
  + Hit Catch 28b23/32 -2 5 0 N 0
  + Hit Catch 28b47/64 -2 5 0 N 0
  + Hit Catch 28b3/4 -2 5 0 N 0
  + Hit Catch 28b49/64 -2 5 0 N 0
  + Hit Catch 28b25/32 -2 5 0 N 0
  + Hit Catch 28b51/64 -2 5 0 N 0
  + Hit Catch 28b13/16 -2 5 0 N 0
  + Hit Catch 28b53/64 -2 5 0 N 0
  + Hit Catch 28b27/32 -2 5 0 N 0
  + Hit Catch 28b55/64 -2 5 0 N 0
  + Hit Catch 28b7/8 -2 5 0 N 0
  + Hit Catch 28b57/64 -2 5 0 N 0
  + Hit Catch 28b29/32 -2 5 0 N 0
  + Hit Catch 28b59/64 -2 5 0 N 0
  + Hit Catch 28b15/16 -2 5 0 N 0
  + Hit Catch 28b61/64 -2 5 0 N 0
  + Hit Catch 28b31/32 -2 5 0 N 0
  + Hit Catch 28b63/64 -2 5 0 N 0
  + Hit Catch 29b -2 5 0 N 0
  + Hit Normal 32b -2 5 1 N 0
  + Hit Catch 32b1/64 -2 5 0 N 0
  + Hit Catch 32b1/32 -2 5 0 N 0
  + Hit Catch 32b3/64 -2 5 0 N 0
  + Hit Catch 32b1/16 -2 5 0 N 0
  + Hit Catch 32b5/64 -2 5 0 N 0
  + Hit Catch 32b3/32 -2 5 0 N 0
  + Hit Catch 32b7/64 -2 5 0 N 0
  + Hit Catch 32b1/8 -2 5 0 N 0
  + Hit Catch 32b9/64 -2 5 0 N 0
  + Hit Catch 32b5/32 -2 5 0 N 0
  + Hit Catch 32b11/64 -2 5 0 N 0
  + Hit Catch 32b3/16 -2 5 0 N 0
  + Hit Catch 32b13/64 -2 5 0 N 0
  + Hit Catch 32b7/32 -2 5 0 N 0
  + Hit Catch 32b15/64 -2 5 0 N 0
  + Hit Catch 32b1/4 -2 5 0 N 0
  + Hit Catch 32b17/64 -2 5 0 N 0
  + Hit Catch 32b9/32 -2 5 0 N 0
  + Hit Catch 32b19/64 -2 5 0 N 0
  + Hit Catch 32b5/16 -2 5 0 N 0
  + Hit Catch 32b21/64 -2 5 0 N 0
  + Hit Catch 32b11/32 -2 5 0 N 0
  + Hit Catch 32b23/64 -2 5 0 N 0
  + Hit Catch 32b3/8 -2 5 0 N 0
  + Hit Catch 32b25/64 -2 5 0 N 0
  + Hit Catch 32b13/32 -2 5 0 N 0
  + Hit Catch 32b27/64 -2 5 0 N 0
  + Hit Catch 32b7/16 -2 5 0 N 0
  + Hit Catch 32b29/64 -2 5 0 N 0
  + Hit Catch 32b15/32 -2 5 0 N 0
  + Hit Catch 32b31/64 -2 5 0 N 0
  + Hit Catch 32b1/2 -2 5 0 N 0
  + Hit Catch 32b33/64 -2 5 0 N 0
  + Hit Catch 32b17/32 -2 5 0 N 0
  + Hit Catch 32b35/64 -2 5 0 N 0
  + Hit Catch 32b9/16 -2 5 0 N 0
  + Hit Catch 32b37/64 -2 5 0 N 0
  + Hit Catch 32b19/32 -2 5 0 N 0
  + Hit Catch 32b39/64 -2 5 0 N 0
  + Hit Catch 32b5/8 -2 5 0 N 0
  + Hit Catch 32b41/64 -2 5 0 N 0
  + Hit Catch 32b21/32 -2 5 0 N 0
  + Hit Catch 32b43/64 -2 5 0 N 0
  + Hit Catch 32b11/16 -2 5 0 N 0
  + Hit Catch 32b45/64 -2 5 0 N 0
  + Hit Catch 32b23/32 -2 5 0 N 0
  + Hit Catch 32b47/64 -2 5 0 N 0
  + Hit Catch 32b3/4 -2 5 0 N 0
  + Hit Catch 32b49/64 -2 5 0 N 0
  + Hit Catch 32b25/32 -2 5 0 N 0
  + Hit Catch 32b51/64 -2 5 0 N 0
  + Hit Catch 32b13/16 -2 5 0 N 0
  + Hit Catch 32b53/64 -2 5 0 N 0
  + Hit Catch 32b27/32 -2 5 0 N 0
  + Hit Catch 32b55/64 -2 5 0 N 0
  + Hit Catch 32b7/8 -2 5 0 N 0
  + Hit Catch 32b57/64 -2 5 0 N 0
  + Hit Catch 32b29/32 -2 5 0 N 0
  + Hit Catch 32b59/64 -2 5 0 N 0
  + Hit Catch 32b15/16 -2 5 0 N 0
  + Hit Catch 32b61/64 -2 5 0 N 0
  + Hit Catch 32b31/32 -2 5 0 N 0
  + Hit Catch 32b63/64 -2 5 0 N 0
  + Hit Catch 33b -2 5 0 N 0
  + Hit Normal 36b -2 5 1 N 0
  + Hit Catch 36b1/64 -2 5 0 N 0
  + Hit Catch 36b1/32 -2 5 0 N 0
  + Hit Catch 36b3/64 -2 5 0 N 0
  + Hit Catch 36b1/16 -2 5 0 N 0
  + Hit Catch 36b5/64 -2 5 0 N 0
  + Hit Catch 36b3/32 -2 5 0 N 0
  + Hit Catch 36b7/64 -2 5 0 N 0
  + Hit Catch 36b1/8 -2 5 0 N 0
  + Hit Catch 36b9/64 -2 5 0 N 0
  + Hit Catch 36b5/32 -2 5 0 N 0
  + Hit Catch 36b11/64 -2 5 0 N 0
  + Hit Catch 36b3/16 -2 5 0 N 0
  + Hit Catch 36b13/64 -2 5 0 N 0
  + Hit Catch 36b7/32 -2 5 0 N 0
  + Hit Catch 36b15/64 -2 5 0 N 0
  + Hit Catch 36b1/4 -2 5 0 N 0
  + Hit Catch 36b17/64 -2 5 0 N 0
  + Hit Catch 36b9/32 -2 5 0 N 0
  + Hit Catch 36b19/64 -2 5 0 N 0
  + Hit Catch 36b5/16 -2 5 0 N 0
  + Hit Catch 36b21/64 -2 5 0 N 0
  + Hit Catch 36b11/32 -2 5 0 N 0
  + Hit Catch 36b23/64 -2 5 0 N 0
  + Hit Catch 36b3/8 -2 5 0 N 0
  + Hit Catch 36b25/64 -2 5 0 N 0
  + Hit Catch 36b13/32 -2 5 0 N 0
  + Hit Catch 36b27/64 -2 5 0 N 0
  + Hit Catch 36b7/16 -2 5 0 N 0
  + Hit Catch 36b29/64 -2 5 0 N 0
  + Hit Catch 36b15/32 -2 5 0 N 0
  + Hit Catch 36b31/64 -2 5 0 N 0
  + Hit Catch 36b1/2 -2 5 0 N 0
  + Hit Catch 36b33/64 -2 5 0 N 0
  + Hit Catch 36b17/32 -2 5 0 N 0
  + Hit Catch 36b35/64 -2 5 0 N 0
  + Hit Catch 36b9/16 -2 5 0 N 0
  + Hit Catch 36b37/64 -2 5 0 N 0
  + Hit Catch 36b19/32 -2 5 0 N 0
  + Hit Catch 36b39/64 -2 5 0 N 0
  + Hit Catch 36b5/8 -2 5 0 N 0
  + Hit Catch 36b41/64 -2 5 0 N 0
  + Hit Catch 36b21/32 -2 5 0 N 0
  + Hit Catch 36b43/64 -2 5 0 N 0
  + Hit Catch 36b11/16 -2 5 0 N 0
  + Hit Catch 36b45/64 -2 5 0 N 0
  + Hit Catch 36b23/32 -2 5 0 N 0
  + Hit Catch 36b47/64 -2 5 0 N 0
  + Hit Catch 36b3/4 -2 5 0 N 0
  + Hit Catch 36b49/64 -2 5 0 N 0
  + Hit Catch 36b25/32 -2 5 0 N 0
  + Hit Catch 36b51/64 -2 5 0 N 0
  + Hit Catch 36b13/16 -2 5 0 N 0
  + Hit Catch 36b53/64 -2 5 0 N 0
  + Hit Catch 36b27/32 -2 5 0 N 0
  + Hit Catch 36b55/64 -2 5 0 N 0
  + Hit Catch 36b7/8 -2 5 0 N 0
  + Hit Catch 36b57/64 -2 5 0 N 0
  + Hit Catch 36b29/32 -2 5 0 N 0
  + Hit Catch 36b59/64 -2 5 0 N 0
  + Hit Catch 36b15/16 -2 5 0 N 0
  + Hit Catch 36b61/64 -2 5 0 N 0
  + Hit Catch 36b31/32 -2 5 0 N 0
  + Hit Catch 36b63/64 -2 5 0 N 0
  + Hit Catch 37b -2 5 0 N 0
  + Hit Normal 40b -2 5 1 N 0
  + Hit Catch 40b1/64 -2 5 0 N 0
  + Hit Catch 40b1/32 -2 5 0 N 0
  + Hit Catch 40b3/64 -2 5 0 N 0
  + Hit Catch 40b1/16 -2 5 0 N 0
  + Hit Catch 40b5/64 -2 5 0 N 0
  + Hit Catch 40b3/32 -2 5 0 N 0
  + Hit Catch 40b7/64 -2 5 0 N 0
  + Hit Catch 40b1/8 -2 5 0 N 0
  + Hit Catch 40b9/64 -2 5 0 N 0
  + Hit Catch 40b5/32 -2 5 0 N 0
  + Hit Catch 40b11/64 -2 5 0 N 0
  + Hit Catch 40b3/16 -2 5 0 N 0
  + Hit Catch 40b13/64 -2 5 0 N 0
  + Hit Catch 40b7/32 -2 5 0 N 0
  + Hit Catch 40b15/64 -2 5 0 N 0
  + Hit Catch 40b1/4 -2 5 0 N 0
  + Hit Catch 40b17/64 -2 5 0 N 0
  + Hit Catch 40b9/32 -2 5 0 N 0
  + Hit Catch 40b19/64 -2 5 0 N 0
  + Hit Catch 40b5/16 -2 5 0 N 0
  + Hit Catch 40b21/64 -2 5 0 N 0
  + Hit Catch 40b11/32 -2 5 0 N 0
  + Hit Catch 40b23/64 -2 5 0 N 0
  + Hit Catch 40b3/8 -2 5 0 N 0
  + Hit Catch 40b25/64 -2 5 0 N 0
  + Hit Catch 40b13/32 -2 5 0 N 0
  + Hit Catch 40b27/64 -2 5 0 N 0
  + Hit Catch 40b7/16 -2 5 0 N 0
  + Hit Catch 40b29/64 -2 5 0 N 0
  + Hit Catch 40b15/32 -2 5 0 N 0
  + Hit Catch 40b31/64 -2 5 0 N 0
  + Hit Catch 40b1/2 -2 5 0 N 0
  + Hit Catch 40b33/64 -2 5 0 N 0
  + Hit Catch 40b17/32 -2 5 0 N 0
  + Hit Catch 40b35/64 -2 5 0 N 0
  + Hit Catch 40b9/16 -2 5 0 N 0
  + Hit Catch 40b37/64 -2 5 0 N 0
  + Hit Catch 40b19/32 -2 5 0 N 0
  + Hit Catch 40b39/64 -2 5 0 N 0
  + Hit Catch 40b5/8 -2 5 0 N 0
  + Hit Catch 40b41/64 -2 5 0 N 0
  + Hit Catch 40b21/32 -2 5 0 N 0
  + Hit Catch 40b43/64 -2 5 0 N 0
  + Hit Catch 40b11/16 -2 5 0 N 0
  + Hit Catch 40b45/64 -2 5 0 N 0
  + Hit Catch 40b23/32 -2 5 0 N 0
  + Hit Catch 40b47/64 -2 5 0 N 0
  + Hit Catch 40b3/4 -2 5 0 N 0
  + Hit Catch 40b49/64 -2 5 0 N 0
  + Hit Catch 40b25/32 -2 5 0 N 0
  + Hit Catch 40b51/64 -2 5 0 N 0
  + Hit Catch 40b13/16 -2 5 0 N 0
  + Hit Catch 40b53/64 -2 5 0 N 0
  + Hit Catch 40b27/32 -2 5 0 N 0
  + Hit Catch 40b55/64 -2 5 0 N 0
  + Hit Catch 40b7/8 -2 5 0 N 0
  + Hit Catch 40b57/64 -2 5 0 N 0
  + Hit Catch 40b29/32 -2 5 0 N 0
  + Hit Catch 40b59/64 -2 5 0 N 0
  + Hit Catch 40b15/16 -2 5 0 N 0
  + Hit Catch 40b61/64 -2 5 0 N 0
  + Hit Catch 40b31/32 -2 5 0 N 0
  + Hit Catch 40b63/64 -2 5 0 N 0
  + Hit Catch 41b -2 5 0 N 0
  + Hit Normal 44b -2 5 1 N 0
  + Hit Catch 44b1/64 -2 5 0 N 0
  + Hit Catch 44b1/32 -2 5 0 N 0
  + Hit Catch 44b3/64 -2 5 0 N 0
  + Hit Catch 44b1/16 -2 5 0 N 0
  + Hit Catch 44b5/64 -2 5 0 N 0
  + Hit Catch 44b3/32 -2 5 0 N 0
  + Hit Catch 44b7/64 -2 5 0 N 0
  + Hit Catch 44b1/8 -2 5 0 N 0
  + Hit Catch 44b9/64 -2 5 0 N 0
  + Hit Catch 44b5/32 -2 5 0 N 0
  + Hit Catch 44b11/64 -2 5 0 N 0
  + Hit Catch 44b3/16 -2 5 0 N 0
  + Hit Catch 44b13/64 -2 5 0 N 0
  + Hit Catch 44b7/32 -2 5 0 N 0
  + Hit Catch 44b15/64 -2 5 0 N 0
  + Hit Catch 44b1/4 -2 5 0 N 0
  + Hit Catch 44b17/64 -2 5 0 N 0
  + Hit Catch 44b9/32 -2 5 0 N 0
  + Hit Catch 44b19/64 -2 5 0 N 0
  + Hit Catch 44b5/16 -2 5 0 N 0
  + Hit Catch 44b21/64 -2 5 0 N 0
  + Hit Catch 44b11/32 -2 5 0 N 0
  + Hit Catch 44b23/64 -2 5 0 N 0
  + Hit Catch 44b3/8 -2 5 0 N 0
  + Hit Catch 44b25/64 -2 5 0 N 0
  + Hit Catch 44b13/32 -2 5 0 N 0
  + Hit Catch 44b27/64 -2 5 0 N 0
  + Hit Catch 44b7/16 -2 5 0 N 0
  + Hit Catch 44b29/64 -2 5 0 N 0
  + Hit Catch 44b15/32 -2 5 0 N 0
  + Hit Catch 44b31/64 -2 5 0 N 0
  + Hit Catch 44b1/2 -2 5 0 N 0
  + Hit Catch 44b33/64 -2 5 0 N 0
  + Hit Catch 44b17/32 -2 5 0 N 0
  + Hit Catch 44b35/64 -2 5 0 N 0
  + Hit Catch 44b9/16 -2 5 0 N 0
  + Hit Catch 44b37/64 -2 5 0 N 0
  + Hit Catch 44b19/32 -2 5 0 N 0
  + Hit Catch 44b39/64 -2 5 0 N 0
  + Hit Catch 44b5/8 -2 5 0 N 0
  + Hit Catch 44b41/64 -2 5 0 N 0
  + Hit Catch 44b21/32 -2 5 0 N 0
  + Hit Catch 44b43/64 -2 5 0 N 0
  + Hit Catch 44b11/16 -2 5 0 N 0
  + Hit Catch 44b45/64 -2 5 0 N 0
  + Hit Catch 44b23/32 -2 5 0 N 0
  + Hit Catch 44b47/64 -2 5 0 N 0
  + Hit Catch 44b3/4 -2 5 0 N 0
  + Hit Catch 44b49/64 -2 5 0 N 0
  + Hit Catch 44b25/32 -2 5 0 N 0
  + Hit Catch 44b51/64 -2 5 0 N 0
  + Hit Catch 44b13/16 -2 5 0 N 0
  + Hit Catch 44b53/64 -2 5 0 N 0
  + Hit Catch 44b27/32 -2 5 0 N 0
  + Hit Catch 44b55/64 -2 5 0 N 0
  + Hit Catch 44b7/8 -2 5 0 N 0
  + Hit Catch 44b57/64 -2 5 0 N 0
  + Hit Catch 44b29/32 -2 5 0 N 0
  + Hit Catch 44b59/64 -2 5 0 N 0
  + Hit Catch 44b15/16 -2 5 0 N 0
  + Hit Catch 44b61/64 -2 5 0 N 0
  + Hit Catch 44b31/32 -2 5 0 N 0
  + Hit Catch 44b63/64 -2 5 0 N 0
  + Hit Catch 45b -2 5 0 N 0
  + Hit Normal 48b -2 5 1 N 0
  + Hit Catch 48b1/64 -2 5 0 N 0
  + Hit Catch 48b1/32 -2 5 0 N 0
  + Hit Catch 48b3/64 -2 5 0 N 0
  + Hit Catch 48b1/16 -2 5 0 N 0
  + Hit Catch 48b5/64 -2 5 0 N 0
  + Hit Catch 48b3/32 -2 5 0 N 0
  + Hit Catch 48b7/64 -2 5 0 N 0
  + Hit Catch 48b1/8 -2 5 0 N 0
  + Hit Catch 48b9/64 -2 5 0 N 0
  + Hit Catch 48b5/32 -2 5 0 N 0
  + Hit Catch 48b11/64 -2 5 0 N 0
  + Hit Catch 48b3/16 -2 5 0 N 0
  + Hit Catch 48b13/64 -2 5 0 N 0
  + Hit Catch 48b7/32 -2 5 0 N 0
  + Hit Catch 48b15/64 -2 5 0 N 0
  + Hit Catch 48b1/4 -2 5 0 N 0
  + Hit Catch 48b17/64 -2 5 0 N 0
  + Hit Catch 48b9/32 -2 5 0 N 0
  + Hit Catch 48b19/64 -2 5 0 N 0
  + Hit Catch 48b5/16 -2 5 0 N 0
  + Hit Catch 48b21/64 -2 5 0 N 0
  + Hit Catch 48b11/32 -2 5 0 N 0
  + Hit Catch 48b23/64 -2 5 0 N 0
  + Hit Catch 48b3/8 -2 5 0 N 0
  + Hit Catch 48b25/64 -2 5 0 N 0
  + Hit Catch 48b13/32 -2 5 0 N 0
  + Hit Catch 48b27/64 -2 5 0 N 0
  + Hit Catch 48b7/16 -2 5 0 N 0
  + Hit Catch 48b29/64 -2 5 0 N 0
  + Hit Catch 48b15/32 -2 5 0 N 0
  + Hit Catch 48b31/64 -2 5 0 N 0
  + Hit Catch 48b1/2 -2 5 0 N 0
  + Hit Catch 48b33/64 -2 5 0 N 0
  + Hit Catch 48b17/32 -2 5 0 N 0
  + Hit Catch 48b35/64 -2 5 0 N 0
  + Hit Catch 48b9/16 -2 5 0 N 0
  + Hit Catch 48b37/64 -2 5 0 N 0
  + Hit Catch 48b19/32 -2 5 0 N 0
  + Hit Catch 48b39/64 -2 5 0 N 0
  + Hit Catch 48b5/8 -2 5 0 N 0
  + Hit Catch 48b41/64 -2 5 0 N 0
  + Hit Catch 48b21/32 -2 5 0 N 0
  + Hit Catch 48b43/64 -2 5 0 N 0
  + Hit Catch 48b11/16 -2 5 0 N 0
  + Hit Catch 48b45/64 -2 5 0 N 0
  + Hit Catch 48b23/32 -2 5 0 N 0
  + Hit Catch 48b47/64 -2 5 0 N 0
  + Hit Catch 48b3/4 -2 5 0 N 0
  + Hit Catch 48b49/64 -2 5 0 N 0
  + Hit Catch 48b25/32 -2 5 0 N 0
  + Hit Catch 48b51/64 -2 5 0 N 0
  + Hit Catch 48b13/16 -2 5 0 N 0
  + Hit Catch 48b53/64 -2 5 0 N 0
  + Hit Catch 48b27/32 -2 5 0 N 0
  + Hit Catch 48b55/64 -2 5 0 N 0
  + Hit Catch 48b7/8 -2 5 0 N 0
  + Hit Catch 48b57/64 -2 5 0 N 0
  + Hit Catch 48b29/32 -2 5 0 N 0
  + Hit Catch 48b59/64 -2 5 0 N 0
  + Hit Catch 48b15/16 -2 5 0 N 0
  + Hit Catch 48b61/64 -2 5 0 N 0
  + Hit Catch 48b31/32 -2 5 0 N 0
  + Hit Catch 48b63/64 -2 5 0 N 0
  + Hit Catch 49b -2 5 0 N 0
  + Hit Normal 52b -2 5 1 N 0
  + Hit Catch 52b1/64 -2 5 0 N 0
  + Hit Catch 52b1/32 -2 5 0 N 0
  + Hit Catch 52b3/64 -2 5 0 N 0
  + Hit Catch 52b1/16 -2 5 0 N 0
  + Hit Catch 52b5/64 -2 5 0 N 0
  + Hit Catch 52b3/32 -2 5 0 N 0
  + Hit Catch 52b7/64 -2 5 0 N 0
  + Hit Catch 52b1/8 -2 5 0 N 0
  + Hit Catch 52b9/64 -2 5 0 N 0
  + Hit Catch 52b5/32 -2 5 0 N 0
  + Hit Catch 52b11/64 -2 5 0 N 0
  + Hit Catch 52b3/16 -2 5 0 N 0
  + Hit Catch 52b13/64 -2 5 0 N 0
  + Hit Catch 52b7/32 -2 5 0 N 0
  + Hit Catch 52b15/64 -2 5 0 N 0
  + Hit Catch 52b1/4 -2 5 0 N 0
  + Hit Catch 52b17/64 -2 5 0 N 0
  + Hit Catch 52b9/32 -2 5 0 N 0
  + Hit Catch 52b19/64 -2 5 0 N 0
  + Hit Catch 52b5/16 -2 5 0 N 0
  + Hit Catch 52b21/64 -2 5 0 N 0
  + Hit Catch 52b11/32 -2 5 0 N 0
  + Hit Catch 52b23/64 -2 5 0 N 0
  + Hit Catch 52b3/8 -2 5 0 N 0
  + Hit Catch 52b25/64 -2 5 0 N 0
  + Hit Catch 52b13/32 -2 5 0 N 0
  + Hit Catch 52b27/64 -2 5 0 N 0
  + Hit Catch 52b7/16 -2 5 0 N 0
  + Hit Catch 52b29/64 -2 5 0 N 0
  + Hit Catch 52b15/32 -2 5 0 N 0
  + Hit Catch 52b31/64 -2 5 0 N 0
  + Hit Catch 52b1/2 -2 5 0 N 0
  + Hit Catch 52b33/64 -2 5 0 N 0
  + Hit Catch 52b17/32 -2 5 0 N 0
  + Hit Catch 52b35/64 -2 5 0 N 0
  + Hit Catch 52b9/16 -2 5 0 N 0
  + Hit Catch 52b37/64 -2 5 0 N 0
  + Hit Catch 52b19/32 -2 5 0 N 0
  + Hit Catch 52b39/64 -2 5 0 N 0
  + Hit Catch 52b5/8 -2 5 0 N 0
  + Hit Catch 52b41/64 -2 5 0 N 0
  + Hit Catch 52b21/32 -2 5 0 N 0
  + Hit Catch 52b43/64 -2 5 0 N 0
  + Hit Catch 52b11/16 -2 5 0 N 0
  + Hit Catch 52b45/64 -2 5 0 N 0
  + Hit Catch 52b23/32 -2 5 0 N 0
  + Hit Catch 52b47/64 -2 5 0 N 0
  + Hit Catch 52b3/4 -2 5 0 N 0
  + Hit Catch 52b49/64 -2 5 0 N 0
  + Hit Catch 52b25/32 -2 5 0 N 0
  + Hit Catch 52b51/64 -2 5 0 N 0
  + Hit Catch 52b13/16 -2 5 0 N 0
  + Hit Catch 52b53/64 -2 5 0 N 0
  + Hit Catch 52b27/32 -2 5 0 N 0
  + Hit Catch 52b55/64 -2 5 0 N 0
  + Hit Catch 52b7/8 -2 5 0 N 0
  + Hit Catch 52b57/64 -2 5 0 N 0
  + Hit Catch 52b29/32 -2 5 0 N 0
  + Hit Catch 52b59/64 -2 5 0 N 0
  + Hit Catch 52b15/16 -2 5 0 N 0
  + Hit Catch 52b61/64 -2 5 0 N 0
  + Hit Catch 52b31/32 -2 5 0 N 0
  + Hit Catch 52b63/64 -2 5 0 N 0
  + Hit Catch 53b -2 5 0 N 0
  + Hit Normal 56b -2 5 1 N 0
  + Hit Catch 56b1/64 -2 5 0 N 0
  + Hit Catch 56b1/32 -2 5 0 N 0
  + Hit Catch 56b3/64 -2 5 0 N 0
  + Hit Catch 56b1/16 -2 5 0 N 0
  + Hit Catch 56b5/64 -2 5 0 N 0
  + Hit Catch 56b3/32 -2 5 0 N 0
  + Hit Catch 56b7/64 -2 5 0 N 0
  + Hit Catch 56b1/8 -2 5 0 N 0
  + Hit Catch 56b9/64 -2 5 0 N 0
  + Hit Catch 56b5/32 -2 5 0 N 0
  + Hit Catch 56b11/64 -2 5 0 N 0
  + Hit Catch 56b3/16 -2 5 0 N 0
  + Hit Catch 56b13/64 -2 5 0 N 0
  + Hit Catch 56b7/32 -2 5 0 N 0
  + Hit Catch 56b15/64 -2 5 0 N 0
  + Hit Catch 56b1/4 -2 5 0 N 0
  + Hit Catch 56b17/64 -2 5 0 N 0
  + Hit Catch 56b9/32 -2 5 0 N 0
  + Hit Catch 56b19/64 -2 5 0 N 0
  + Hit Catch 56b5/16 -2 5 0 N 0
  + Hit Catch 56b21/64 -2 5 0 N 0
  + Hit Catch 56b11/32 -2 5 0 N 0
  + Hit Catch 56b23/64 -2 5 0 N 0
  + Hit Catch 56b3/8 -2 5 0 N 0
  + Hit Catch 56b25/64 -2 5 0 N 0
  + Hit Catch 56b13/32 -2 5 0 N 0
  + Hit Catch 56b27/64 -2 5 0 N 0
  + Hit Catch 56b7/16 -2 5 0 N 0
  + Hit Catch 56b29/64 -2 5 0 N 0
  + Hit Catch 56b15/32 -2 5 0 N 0
  + Hit Catch 56b31/64 -2 5 0 N 0
  + Hit Catch 56b1/2 -2 5 0 N 0
  + Hit Catch 56b33/64 -2 5 0 N 0
  + Hit Catch 56b17/32 -2 5 0 N 0
  + Hit Catch 56b35/64 -2 5 0 N 0
  + Hit Catch 56b9/16 -2 5 0 N 0
  + Hit Catch 56b37/64 -2 5 0 N 0
  + Hit Catch 56b19/32 -2 5 0 N 0
  + Hit Catch 56b39/64 -2 5 0 N 0
  + Hit Catch 56b5/8 -2 5 0 N 0
  + Hit Catch 56b41/64 -2 5 0 N 0
  + Hit Catch 56b21/32 -2 5 0 N 0
  + Hit Catch 56b43/64 -2 5 0 N 0
  + Hit Catch 56b11/16 -2 5 0 N 0
  + Hit Catch 56b45/64 -2 5 0 N 0
  + Hit Catch 56b23/32 -2 5 0 N 0
  + Hit Catch 56b47/64 -2 5 0 N 0
  + Hit Catch 56b3/4 -2 5 0 N 0
  + Hit Catch 56b49/64 -2 5 0 N 0
  + Hit Catch 56b25/32 -2 5 0 N 0
  + Hit Catch 56b51/64 -2 5 0 N 0
  + Hit Catch 56b13/16 -2 5 0 N 0
  + Hit Catch 56b53/64 -2 5 0 N 0
  + Hit Catch 56b27/32 -2 5 0 N 0
  + Hit Catch 56b55/64 -2 5 0 N 0
  + Hit Catch 56b7/8 -2 5 0 N 0
  + Hit Catch 56b57/64 -2 5 0 N 0
  + Hit Catch 56b29/32 -2 5 0 N 0
  + Hit Catch 56b59/64 -2 5 0 N 0
  + Hit Catch 56b15/16 -2 5 0 N 0
  + Hit Catch 56b61/64 -2 5 0 N 0
  + Hit Catch 56b31/32 -2 5 0 N 0
  + Hit Catch 56b63/64 -2 5 0 N 0
  + Hit Catch 57b -2 5 0 N 0
  + Hit Normal 60b -2 5 1 N 0
  + Hit Catch 60b1/64 -2 5 0 N 0
  + Hit Catch 60b1/32 -2 5 0 N 0
  + Hit Catch 60b3/64 -2 5 0 N 0
  + Hit Catch 60b1/16 -2 5 0 N 0
  + Hit Catch 60b5/64 -2 5 0 N 0
  + Hit Catch 60b3/32 -2 5 0 N 0
  + Hit Catch 60b7/64 -2 5 0 N 0
  + Hit Catch 60b1/8 -2 5 0 N 0
  + Hit Catch 60b9/64 -2 5 0 N 0
  + Hit Catch 60b5/32 -2 5 0 N 0
  + Hit Catch 60b11/64 -2 5 0 N 0
  + Hit Catch 60b3/16 -2 5 0 N 0
  + Hit Catch 60b13/64 -2 5 0 N 0
  + Hit Catch 60b7/32 -2 5 0 N 0
  + Hit Catch 60b15/64 -2 5 0 N 0
  + Hit Catch 60b1/4 -2 5 0 N 0
  + Hit Catch 60b17/64 -2 5 0 N 0
  + Hit Catch 60b9/32 -2 5 0 N 0
  + Hit Catch 60b19/64 -2 5 0 N 0
  + Hit Catch 60b5/16 -2 5 0 N 0
  + Hit Catch 60b21/64 -2 5 0 N 0
  + Hit Catch 60b11/32 -2 5 0 N 0
  + Hit Catch 60b23/64 -2 5 0 N 0
  + Hit Catch 60b3/8 -2 5 0 N 0
  + Hit Catch 60b25/64 -2 5 0 N 0
  + Hit Catch 60b13/32 -2 5 0 N 0
  + Hit Catch 60b27/64 -2 5 0 N 0
  + Hit Catch 60b7/16 -2 5 0 N 0
  + Hit Catch 60b29/64 -2 5 0 N 0
  + Hit Catch 60b15/32 -2 5 0 N 0
  + Hit Catch 60b31/64 -2 5 0 N 0
  + Hit Catch 60b1/2 -2 5 0 N 0
  + Hit Catch 60b33/64 -2 5 0 N 0
  + Hit Catch 60b17/32 -2 5 0 N 0
  + Hit Catch 60b35/64 -2 5 0 N 0
  + Hit Catch 60b9/16 -2 5 0 N 0
  + Hit Catch 60b37/64 -2 5 0 N 0
  + Hit Catch 60b19/32 -2 5 0 N 0
  + Hit Catch 60b39/64 -2 5 0 N 0
  + Hit Catch 60b5/8 -2 5 0 N 0
  + Hit Catch 60b41/64 -2 5 0 N 0
  + Hit Catch 60b21/32 -2 5 0 N 0
  + Hit Catch 60b43/64 -2 5 0 N 0
  + Hit Catch 60b11/16 -2 5 0 N 0
  + Hit Catch 60b45/64 -2 5 0 N 0
  + Hit Catch 60b23/32 -2 5 0 N 0
  + Hit Catch 60b47/64 -2 5 0 N 0
  + Hit Catch 60b3/4 -2 5 0 N 0
  + Hit Catch 60b49/64 -2 5 0 N 0
  + Hit Catch 60b25/32 -2 5 0 N 0
  + Hit Catch 60b51/64 -2 5 0 N 0
  + Hit Catch 60b13/16 -2 5 0 N 0
  + Hit Catch 60b53/64 -2 5 0 N 0
  + Hit Catch 60b27/32 -2 5 0 N 0
  + Hit Catch 60b55/64 -2 5 0 N 0
  + Hit Catch 60b7/8 -2 5 0 N 0
  + Hit Catch 60b57/64 -2 5 0 N 0
  + Hit Catch 60b29/32 -2 5 0 N 0
  + Hit Catch 60b59/64 -2 5 0 N 0
  + Hit Catch 60b15/16 -2 5 0 N 0
  + Hit Catch 60b61/64 -2 5 0 N 0
  + Hit Catch 60b31/32 -2 5 0 N 0
  + Hit Catch 60b63/64 -2 5 0 N 0
  + Hit Catch 61b -2 5 0 N 0
  + Hit Normal 64b -2 5 1 N 0
  + Hit Catch 64b1/64 -2 5 0 N 0
  + Hit Catch 64b1/32 -2 5 0 N 0
  + Hit Catch 64b3/64 -2 5 0 N 0
  + Hit Catch 64b1/16 -2 5 0 N 0
  + Hit Catch 64b5/64 -2 5 0 N 0
  + Hit Catch 64b3/32 -2 5 0 N 0
  + Hit Catch 64b7/64 -2 5 0 N 0
  + Hit Catch 64b1/8 -2 5 0 N 0
  + Hit Catch 64b9/64 -2 5 0 N 0
  + Hit Catch 64b5/32 -2 5 0 N 0
  + Hit Catch 64b11/64 -2 5 0 N 0
  + Hit Catch 64b3/16 -2 5 0 N 0
  + Hit Catch 64b13/64 -2 5 0 N 0
  + Hit Catch 64b7/32 -2 5 0 N 0
  + Hit Catch 64b15/64 -2 5 0 N 0
  + Hit Catch 64b1/4 -2 5 0 N 0
  + Hit Catch 64b17/64 -2 5 0 N 0
  + Hit Catch 64b9/32 -2 5 0 N 0
  + Hit Catch 64b19/64 -2 5 0 N 0
  + Hit Catch 64b5/16 -2 5 0 N 0
  + Hit Catch 64b21/64 -2 5 0 N 0
  + Hit Catch 64b11/32 -2 5 0 N 0
  + Hit Catch 64b23/64 -2 5 0 N 0
  + Hit Catch 64b3/8 -2 5 0 N 0
  + Hit Catch 64b25/64 -2 5 0 N 0
  + Hit Catch 64b13/32 -2 5 0 N 0
  + Hit Catch 64b27/64 -2 5 0 N 0
  + Hit Catch 64b7/16 -2 5 0 N 0
  + Hit Catch 64b29/64 -2 5 0 N 0
  + Hit Catch 64b15/32 -2 5 0 N 0
  + Hit Catch 64b31/64 -2 5 0 N 0
  + Hit Catch 64b1/2 -2 5 0 N 0
  + Hit Catch 64b33/64 -2 5 0 N 0
  + Hit Catch 64b17/32 -2 5 0 N 0
  + Hit Catch 64b35/64 -2 5 0 N 0
  + Hit Catch 64b9/16 -2 5 0 N 0
  + Hit Catch 64b37/64 -2 5 0 N 0
  + Hit Catch 64b19/32 -2 5 0 N 0
  + Hit Catch 64b39/64 -2 5 0 N 0
  + Hit Catch 64b5/8 -2 5 0 N 0
  + Hit Catch 64b41/64 -2 5 0 N 0
  + Hit Catch 64b21/32 -2 5 0 N 0
  + Hit Catch 64b43/64 -2 5 0 N 0
  + Hit Catch 64b11/16 -2 5 0 N 0
  + Hit Catch 64b45/64 -2 5 0 N 0
  + Hit Catch 64b23/32 -2 5 0 N 0
  + Hit Catch 64b47/64 -2 5 0 N 0
  + Hit Catch 64b3/4 -2 5 0 N 0
  + Hit Catch 64b49/64 -2 5 0 N 0
  + Hit Catch 64b25/32 -2 5 0 N 0
  + Hit Catch 64b51/64 -2 5 0 N 0
  + Hit Catch 64b13/16 -2 5 0 N 0
  + Hit Catch 64b53/64 -2 5 0 N 0
  + Hit Catch 64b27/32 -2 5 0 N 0
  + Hit Catch 64b55/64 -2 5 0 N 0
  + Hit Catch 64b7/8 -2 5 0 N 0
  + Hit Catch 64b57/64 -2 5 0 N 0
  + Hit Catch 64b29/32 -2 5 0 N 0
  + Hit Catch 64b59/64 -2 5 0 N 0
  + Hit Catch 64b15/16 -2 5 0 N 0
  + Hit Catch 64b61/64 -2 5 0 N 0
  + Hit Catch 64b31/32 -2 5 0 N 0
  + Hit Catch 64b63/64 -2 5 0 N 0
  + Hit Catch 65b -2 5 0 N 0
  + Hit Normal 68b -2 5 1 N 0
  + Hit Catch 68b1/64 -2 5 0 N 0
  + Hit Catch 68b1/32 -2 5 0 N 0
  + Hit Catch 68b3/64 -2 5 0 N 0
  + Hit Catch 68b1/16 -2 5 0 N 0
  + Hit Catch 68b5/64 -2 5 0 N 0
  + Hit Catch 68b3/32 -2 5 0 N 0
  + Hit Catch 68b7/64 -2 5 0 N 0
  + Hit Catch 68b1/8 -2 5 0 N 0
  + Hit Catch 68b9/64 -2 5 0 N 0
  + Hit Catch 68b5/32 -2 5 0 N 0
  + Hit Catch 68b11/64 -2 5 0 N 0
  + Hit Catch 68b3/16 -2 5 0 N 0
  + Hit Catch 68b13/64 -2 5 0 N 0
  + Hit Catch 68b7/32 -2 5 0 N 0
  + Hit Catch 68b15/64 -2 5 0 N 0
  + Hit Catch 68b1/4 -2 5 0 N 0
  + Hit Catch 68b17/64 -2 5 0 N 0
  + Hit Catch 68b9/32 -2 5 0 N 0
  + Hit Catch 68b19/64 -2 5 0 N 0
  + Hit Catch 68b5/16 -2 5 0 N 0
  + Hit Catch 68b21/64 -2 5 0 N 0
  + Hit Catch 68b11/32 -2 5 0 N 0
  + Hit Catch 68b23/64 -2 5 0 N 0
  + Hit Catch 68b3/8 -2 5 0 N 0
  + Hit Catch 68b25/64 -2 5 0 N 0
  + Hit Catch 68b13/32 -2 5 0 N 0
  + Hit Catch 68b27/64 -2 5 0 N 0
  + Hit Catch 68b7/16 -2 5 0 N 0
  + Hit Catch 68b29/64 -2 5 0 N 0
  + Hit Catch 68b15/32 -2 5 0 N 0
  + Hit Catch 68b31/64 -2 5 0 N 0
  + Hit Catch 68b1/2 -2 5 0 N 0
  + Hit Catch 68b33/64 -2 5 0 N 0
  + Hit Catch 68b17/32 -2 5 0 N 0
  + Hit Catch 68b35/64 -2 5 0 N 0
  + Hit Catch 68b9/16 -2 5 0 N 0
  + Hit Catch 68b37/64 -2 5 0 N 0
  + Hit Catch 68b19/32 -2 5 0 N 0
  + Hit Catch 68b39/64 -2 5 0 N 0
  + Hit Catch 68b5/8 -2 5 0 N 0
  + Hit Catch 68b41/64 -2 5 0 N 0
  + Hit Catch 68b21/32 -2 5 0 N 0
  + Hit Catch 68b43/64 -2 5 0 N 0
  + Hit Catch 68b11/16 -2 5 0 N 0
  + Hit Catch 68b45/64 -2 5 0 N 0
  + Hit Catch 68b23/32 -2 5 0 N 0
  + Hit Catch 68b47/64 -2 5 0 N 0
  + Hit Catch 68b3/4 -2 5 0 N 0
  + Hit Catch 68b49/64 -2 5 0 N 0
  + Hit Catch 68b25/32 -2 5 0 N 0
  + Hit Catch 68b51/64 -2 5 0 N 0
  + Hit Catch 68b13/16 -2 5 0 N 0
  + Hit Catch 68b53/64 -2 5 0 N 0
  + Hit Catch 68b27/32 -2 5 0 N 0
  + Hit Catch 68b55/64 -2 5 0 N 0
  + Hit Catch 68b7/8 -2 5 0 N 0
  + Hit Catch 68b57/64 -2 5 0 N 0
  + Hit Catch 68b29/32 -2 5 0 N 0
  + Hit Catch 68b59/64 -2 5 0 N 0
  + Hit Catch 68b15/16 -2 5 0 N 0
  + Hit Catch 68b61/64 -2 5 0 N 0
  + Hit Catch 68b31/32 -2 5 0 N 0
  + Hit Catch 68b63/64 -2 5 0 N 0
  + Hit Catch 69b -2 5 0 N 0
  + Hit Normal 72b -2 5 1 N 0
  + Hit Catch 72b1/64 -2 5 0 N 0
  + Hit Catch 72b1/32 -2 5 0 N 0
  + Hit Catch 72b3/64 -2 5 0 N 0
  + Hit Catch 72b1/16 -2 5 0 N 0
  + Hit Catch 72b5/64 -2 5 0 N 0
  + Hit Catch 72b3/32 -2 5 0 N 0
  + Hit Catch 72b7/64 -2 5 0 N 0
  + Hit Catch 72b1/8 -2 5 0 N 0
  + Hit Catch 72b9/64 -2 5 0 N 0
  + Hit Catch 72b5/32 -2 5 0 N 0
  + Hit Catch 72b11/64 -2 5 0 N 0
  + Hit Catch 72b3/16 -2 5 0 N 0
  + Hit Catch 72b13/64 -2 5 0 N 0
  + Hit Catch 72b7/32 -2 5 0 N 0
  + Hit Catch 72b15/64 -2 5 0 N 0
  + Hit Catch 72b1/4 -2 5 0 N 0
  + Hit Catch 72b17/64 -2 5 0 N 0
  + Hit Catch 72b9/32 -2 5 0 N 0
  + Hit Catch 72b19/64 -2 5 0 N 0
  + Hit Catch 72b5/16 -2 5 0 N 0
  + Hit Catch 72b21/64 -2 5 0 N 0
  + Hit Catch 72b11/32 -2 5 0 N 0
  + Hit Catch 72b23/64 -2 5 0 N 0
  + Hit Catch 72b3/8 -2 5 0 N 0
  + Hit Catch 72b25/64 -2 5 0 N 0
  + Hit Catch 72b13/32 -2 5 0 N 0
  + Hit Catch 72b27/64 -2 5 0 N 0
  + Hit Catch 72b7/16 -2 5 0 N 0
  + Hit Catch 72b29/64 -2 5 0 N 0
  + Hit Catch 72b15/32 -2 5 0 N 0
  + Hit Catch 72b31/64 -2 5 0 N 0
  + Hit Catch 72b1/2 -2 5 0 N 0
  + Hit Catch 72b33/64 -2 5 0 N 0
  + Hit Catch 72b17/32 -2 5 0 N 0
  + Hit Catch 72b35/64 -2 5 0 N 0
  + Hit Catch 72b9/16 -2 5 0 N 0
  + Hit Catch 72b37/64 -2 5 0 N 0
  + Hit Catch 72b19/32 -2 5 0 N 0
  + Hit Catch 72b39/64 -2 5 0 N 0
  + Hit Catch 72b5/8 -2 5 0 N 0
  + Hit Catch 72b41/64 -2 5 0 N 0
  + Hit Catch 72b21/32 -2 5 0 N 0
  + Hit Catch 72b43/64 -2 5 0 N 0
  + Hit Catch 72b11/16 -2 5 0 N 0
  + Hit Catch 72b45/64 -2 5 0 N 0
  + Hit Catch 72b23/32 -2 5 0 N 0
  + Hit Catch 72b47/64 -2 5 0 N 0
  + Hit Catch 72b3/4 -2 5 0 N 0
  + Hit Catch 72b49/64 -2 5 0 N 0
  + Hit Catch 72b25/32 -2 5 0 N 0
  + Hit Catch 72b51/64 -2 5 0 N 0
  + Hit Catch 72b13/16 -2 5 0 N 0
  + Hit Catch 72b53/64 -2 5 0 N 0
  + Hit Catch 72b27/32 -2 5 0 N 0
  + Hit Catch 72b55/64 -2 5 0 N 0
  + Hit Catch 72b7/8 -2 5 0 N 0
  + Hit Catch 72b57/64 -2 5 0 N 0
  + Hit Catch 72b29/32 -2 5 0 N 0
  + Hit Catch 72b59/64 -2 5 0 N 0
  + Hit Catch 72b15/16 -2 5 0 N 0
  + Hit Catch 72b61/64 -2 5 0 N 0
  + Hit Catch 72b31/32 -2 5 0 N 0
  + Hit Catch 72b63/64 -2 5 0 N 0
  + Hit Catch 73b -2 5 0 N 0
  + Hit Normal 76b 0.35 0.25 0.75 N 0
  + Hit Normal 76b 0.65 0.25 0.75 N 0
  + Hit Normal 77b 0.35 0.25 0.75 N 0
  + Hit Normal 77b 0.65 0.25 0.75 N 0
  + Hit Normal 78b 0.35 0.25 0.75 N 0
  + Hit Normal 78b 0.65 0.25 0.75 N 0
  + Hit Normal 79b 0.65 0.25 0.75 N 0
  + Hit Normal 79b 0.35 0.25 0.75 N 0
  + Hit Normal 80b 0.35 0.25 0.75 N 0
  + Hit Normal 80b 0.04999999 0.25 0.75 N 0
  + Hit Normal 81b 0.35 0.25 0.75 N 0
  + Hit Normal 81b 0.04999999 0.25 0.75 N 0
  + Hit Normal 82b 0.35 0.25 0.75 N 0
  + Hit Normal 82b 0.04999999 0.25 0.75 N 0
  + Hit Normal 83b 0.35 0.25 0.75 N 0
  + Hit Normal 83b 0.04999999 0.25 0.75 N 0
  + Hit Normal 84b 0.7 0.25 0.75 N 0
  + Hit Normal 84b 0.04999999 0.25 0.75 N 0
  + Hit Normal 85b 0.7 0.25 0.75 N 0
  + Hit Normal 85b 0.04999999 0.25 0.75 N 0
  + Hit Normal 86b 0.7 0.25 0.75 N 0
  + Hit Normal 86b 0.04999999 0.25 0.75 N 0
  + Hit Normal 87b 0.04999999 0.25 0.75 N 0
  + Hit Normal 87b 0.7 0.25 0.75 N 0
  + Hit Normal 88b 0.2 0.25 0.75 N 0
  + Hit Normal 88b 0.5500001 0.25 0.75 N 0
  + Hit Normal 89b 0.5500001 0.25 0.75 N 0
  + Hit Normal 89b 0.2 0.25 0.75 N 0
  + Hit Normal 90b 0.5500001 0.25 0.75 N 0
  + Hit Normal 90b 0.2 0.25 0.75 N 0
  + Hit Normal 92b 0.2 0.25 0.75 N 0
  + Hit Normal 92b 0.5500001 0.25 0.75 N 0
  + Hit Normal 93b 0.2 0.25 0.75 N 0
  + Hit Normal 93b 0.5500001 0.25 0.75 N 0
  + Hit Normal 94b 0.5500001 0.25 0.75 N 0
  + Hit Normal 94b 0.2 0.25 0.75 N 0
  + Hit Normal 95b 0.2 0.25 0.75 N 0
  + Hit Normal 95b 0.5500001 0.25 0.75 N 0
  + Hit Normal 96b 0.04999999 0.25 0.75 N 0
  + Hit Normal 96b 0.7 0.25 0.75 N 0
  + Hit Normal 97b 0.7 0.25 0.75 N 0
  + Hit Normal 97b 0.04999999 0.25 0.75 N 0
  + Hit Normal 98b 0.7 0.25 0.75 N 0
  + Hit Normal 98b 0.04999999 0.25 0.75 N 0
  + Hit Normal 99b 0.7 0.25 0.75 N 0
  + Hit Normal 99b 0.04999999 0.25 0.75 N 0
  + Hit Normal 100b 0.35 0.25 0.75 N 0
  + Hit Normal 100b 0.65 0.25 0.75 N 0
  + Hit Normal 101b 0.35 0.25 0.75 N 0
  + Hit Normal 101b 0.65 0.25 0.75 N 0
  + Hit Normal 102b 0.35 0.25 0.75 N 0
  + Hit Normal 102b 0.65 0.25 0.75 N 0
  + Hit Normal 103b 0.65 0.25 0.75 N 0
  + Hit Normal 103b 0.35 0.25 0.75 N 0
  + Hit Normal 104b 0.04999999 0.25 0.75 N 0
  + Hit Normal 104b 0.35 0.25 0.75 N 0
  + Hit Normal 105b 0.35 0.25 0.75 N 0
  + Hit Normal 105b 0.04999999 0.25 0.75 N 0
  + Hit Normal 106b 0.35 0.25 0.75 N 0
  + Hit Normal 106b 0.04999999 0.25 0.75 N 0
  + Hit Normal 107b 0.35 0.25 0 F180 0
  + Hit Normal 107b 0.04999999 0.25 0 F180 0
+ Lane 0 -4 0 0 0 0 0
  $ Offset_Y 76b 4 4 _ Circle/Out
  + LaneStep 76b -8 0 Linear Linear 8 0 Linear Linear 5
  + LaneStep 107b -8 0 Linear Linear 8 0 Linear Linear 1
  + Hit Normal 78b1/2 0.05 0.25 0 N 0
  + Hit Normal 78b5/6 0.65 0.25 0 N 0
  + Hit Normal 79b 0.3 0.25 0 N 0
  + Hit Normal 79b1/3 0.5 0.25 0 N 0
  + Hit Normal 79b1/2 0.15 0.25 0 N 0
  + Hit Normal 79b5/6 0.6 0.25 0 N 0
  + Hit Catch 80b1/2 0.05 0.25 0 N 0
  + Hit Catch 80b5/6 0.65 0.25 0 N 0
  + Hit Catch 81b 0.3 0.25 0 N 0
  + Hit Catch 81b1/3 0.5 0.25 0 N 0
  + Hit Catch 81b1/2 0.15 0.25 0 N 0
  + Hit Catch 81b5/6 0.6 0.25 0 N 0
  + Hit Normal 82b1/6 0.2 0.25 0.5 N 0
  + Hit Normal 83b 0.525 0.25 0.25 N 0
  + Hit Normal 83b1/2 0.325 0.25 0.25 N 0
  + Hit Normal 84b 0.7 0.25 1.5 N 0
  + Hit Catch 84b1/6 0.2 0.25 0.5 N 0
  + Hit Catch 85b 0.525 0.25 0.25 N 0
  + Hit Catch 85b1/2 0.325 0.25 0.25 N 0
  + Hit Catch 86b 0.7 0.25 1.5 N 0
  + Hit Normal 94b1/2 0.7 0.25 0 N 0
  + Hit Normal 94b5/6 0.1 0.25 0 N 0
  + Hit Normal 95b 0.45 0.25 0 N 0
  + Hit Normal 95b1/3 0.25 0.25 0 N 0
  + Hit Normal 95b1/2 0.6 0.25 0 N 0
  + Hit Normal 95b5/6 0.15 0.25 0 N 0
  + Hit Normal 98b1/6 0.55 0.25 0.5 N 0
  + Hit Normal 99b 0.225 0.25 0.25 N 0
  + Hit Normal 99b1/2 0.425 0.25 0.25 N 0
  + Hit Normal 100b 0.05000001 0.25 1.5 N 0
+ Lane 0 1000 0 0 0 0 -1
  $ Offset_Y 91b 0 -4 _ Linear
  + LaneStep 91b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 92b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 107b -8 0 Linear Linear 8 0 Linear Linear 10
  + Hit Catch 92b -0.4 1.8 0 N 0
  + Hit Catch 92b1/32 -0.45 1 0 N 0
  + Hit Catch 92b1/16 -0.25 1 0 N 0
  + Hit Catch 92b3/32 -0.3 1 0 N 0
  + Hit Catch 92b1/8 -0.1 1 0 N 0
  + Hit Catch 92b5/32 -0.15 1 0 N 0
  + Hit Catch 92b3/16 0.05 1 0 N 0
  + Hit Catch 92b7/32 0 1 0 N 0
  + Hit Catch 92b1/4 -0.3 1 0 N 0
  + Hit Catch 92b9/32 -0.35 1 0 N 0
  + Hit Catch 92b5/16 -0.15 1 0 N 0
  + Hit Catch 92b11/32 -0.2 1 0 N 0
  + Hit Catch 92b3/8 0 1 0 N 0
  + Hit Catch 92b13/32 -0.05 1 0 N 0
  + Hit Catch 92b7/16 0.15 1 0 N 0
  + Hit Catch 92b15/32 0.1 1 0 N 0
  + Hit Catch 92b1/2 -0.2 1 0 N 0
  + Hit Catch 92b17/32 -0.25 1 0 N 0
  + Hit Catch 92b9/16 -0.05 1 0 N 0
  + Hit Catch 92b19/32 -0.1 1 0 N 0
  + Hit Catch 92b5/8 0.1 1 0 N 0
  + Hit Catch 92b21/32 0.05 1 0 N 0
  + Hit Catch 92b11/16 0.25 1 0 N 0
  + Hit Catch 92b23/32 0.2 1 0 N 0
  + Hit Catch 92b3/4 -0.1 1 0 N 0
  + Hit Catch 92b25/32 -0.15 1 0 N 0
  + Hit Catch 92b13/16 0.05 1 0 N 0
  + Hit Catch 92b27/32 0 1 0 N 0
  + Hit Catch 92b7/8 0.2 1 0 N 0
  + Hit Catch 92b29/32 0.15 1 0 N 0
  + Hit Catch 92b15/16 0.35 1 0 N 0
  + Hit Catch 92b31/32 0.3 1 0 N 0
  + Hit Catch 93b -0.4 1 0 N 0
  + Hit Catch 93b1/32 -0.45 1 0 N 0
  + Hit Catch 93b1/16 -0.25 1 0 N 0
  + Hit Catch 93b3/32 -0.3 1 0 N 0
  + Hit Catch 93b1/8 -0.1 1 0 N 0
  + Hit Catch 93b5/32 -0.15 1 0 N 0
  + Hit Catch 93b3/16 0.05 1 0 N 0
  + Hit Catch 93b7/32 0 1 0 N 0
  + Hit Catch 93b1/4 -0.3 1 0 N 0
  + Hit Catch 93b9/32 -0.35 1 0 N 0
  + Hit Catch 93b5/16 -0.15 1 0 N 0
  + Hit Catch 93b11/32 -0.2 1 0 N 0
  + Hit Catch 93b3/8 0 1 0 N 0
  + Hit Catch 93b13/32 -0.05 1 0 N 0
  + Hit Catch 93b7/16 0.15 1 0 N 0
  + Hit Catch 93b15/32 0.1 1 0 N 0
  + Hit Catch 93b1/2 -0.2 1 0 N 0
  + Hit Catch 93b17/32 -0.25 1 0 N 0
  + Hit Catch 93b9/16 -0.05 1 0 N 0
  + Hit Catch 93b19/32 -0.1 1 0 N 0
  + Hit Catch 93b5/8 0.1 1 0 N 0
  + Hit Catch 93b21/32 0.05 1 0 N 0
  + Hit Catch 93b11/16 0.25 1 0 N 0
  + Hit Catch 93b23/32 0.2 1 0 N 0
  + Hit Catch 93b3/4 -0.1 1 0 N 0
  + Hit Catch 93b25/32 -0.15 1 0 N 0
  + Hit Catch 93b13/16 0.05 1 0 N 0
  + Hit Catch 93b27/32 0 1 0 N 0
  + Hit Catch 93b7/8 0.2 1 0 N 0
  + Hit Catch 93b29/32 0.15 1 0 N 0
  + Hit Catch 93b15/16 0.35 1 0 N 0
  + Hit Catch 93b31/32 0.3 1 0 N 0
  + Hit Catch 94b -0.4 1 0 N 0
  + Hit Catch 94b1/32 -0.45 1 0 N 0
  + Hit Catch 94b1/16 -0.25 1 0 N 0
  + Hit Catch 94b3/32 -0.3 1 0 N 0
  + Hit Catch 94b1/8 -0.1 1 0 N 0
  + Hit Catch 94b5/32 -0.15 1 0 N 0
  + Hit Catch 94b3/16 0.05 1 0 N 0
  + Hit Catch 94b7/32 0 1 0 N 0
  + Hit Catch 94b1/4 -0.3 1 0 N 0
  + Hit Catch 94b9/32 -0.35 1 0 N 0
  + Hit Catch 94b5/16 -0.15 1 0 N 0
  + Hit Catch 94b11/32 -0.2 1 0 N 0
  + Hit Catch 94b3/8 0 1 0 N 0
  + Hit Catch 94b13/32 -0.05 1 0 N 0
  + Hit Catch 94b7/16 0.15 1 0 N 0
  + Hit Catch 94b15/32 0.1 1 0 N 0
  + Hit Catch 94b1/2 -0.2 1 0 N 0
  + Hit Catch 94b17/32 -0.25 1 0 N 0
  + Hit Catch 94b9/16 -0.05 1 0 N 0
  + Hit Catch 94b19/32 -0.1 1 0 N 0
  + Hit Catch 94b5/8 0.1 1 0 N 0
  + Hit Catch 94b21/32 0.05 1 0 N 0
  + Hit Catch 94b11/16 0.25 1 0 N 0
  + Hit Catch 94b23/32 0.2 1 0 N 0
  + Hit Catch 94b3/4 -0.1 1 0 N 0
  + Hit Catch 94b25/32 -0.15 1 0 N 0
  + Hit Catch 94b13/16 0.05 1 0 N 0
  + Hit Catch 94b27/32 0 1 0 N 0
  + Hit Catch 94b7/8 0.2 1 0 N 0
  + Hit Catch 94b29/32 0.15 1 0 N 0
  + Hit Catch 94b15/16 0.35 1 0 N 0
  + Hit Catch 94b31/32 0.3 1 0 N 0
  + Hit Catch 95b -0.4 1 0 N 0
  + Hit Catch 95b1/32 -0.45 1 0 N 0
  + Hit Catch 95b1/16 -0.25 1 0 N 0
  + Hit Catch 95b3/32 -0.3 1 0 N 0
  + Hit Catch 95b1/8 -0.1 1 0 N 0
  + Hit Catch 95b5/32 -0.15 1 0 N 0
  + Hit Catch 95b3/16 0.05 1 0 N 0
  + Hit Catch 95b7/32 0 1 0 N 0
  + Hit Catch 95b1/4 -0.3 1 0 N 0
  + Hit Catch 95b9/32 -0.35 1 0 N 0
  + Hit Catch 95b5/16 -0.15 1 0 N 0
  + Hit Catch 95b11/32 -0.2 1 0 N 0
  + Hit Catch 95b3/8 0 1 0 N 0
  + Hit Catch 95b13/32 -0.05 1 0 N 0
  + Hit Catch 95b7/16 0.15 1 0 N 0
  + Hit Catch 95b15/32 0.1 1 0 N 0
  + Hit Catch 95b1/2 -0.2 1 0 N 0
  + Hit Catch 95b17/32 -0.25 1 0 N 0
  + Hit Catch 95b9/16 -0.05 1 0 N 0
  + Hit Catch 95b19/32 -0.1 1 0 N 0
  + Hit Catch 95b5/8 0.1 1 0 N 0
  + Hit Catch 95b21/32 0.05 1 0 N 0
  + Hit Catch 95b11/16 0.25 1 0 N 0
  + Hit Catch 95b23/32 0.2 1 0 N 0
  + Hit Catch 95b3/4 -0.1 1 0 N 0
  + Hit Catch 95b25/32 -0.15 1 0 N 0
  + Hit Catch 95b13/16 0.05 1 0 N 0
  + Hit Catch 95b27/32 0 1 0 N 0
  + Hit Catch 95b7/8 0.2 1 0 N 0
  + Hit Catch 95b29/32 0.15 1 0 N 0
  + Hit Catch 95b15/16 0.35 1 0 N 0
  + Hit Catch 95b31/32 0.3 1 0 N 0
  + Hit Catch 96b -0.4 1 0 N 0
  + Hit Catch 96b1/32 -0.45 1 0 N 0
  + Hit Catch 96b1/16 -0.25 1 0 N 0
  + Hit Catch 96b3/32 -0.3 1 0 N 0
  + Hit Catch 96b1/8 -0.1 1 0 N 0
  + Hit Catch 96b5/32 -0.15 1 0 N 0
  + Hit Catch 96b3/16 0.05 1 0 N 0
  + Hit Catch 96b7/32 0 1 0 N 0
  + Hit Catch 96b1/4 -0.3 1 0 N 0
  + Hit Catch 96b9/32 -0.35 1 0 N 0
  + Hit Catch 96b5/16 -0.15 1 0 N 0
  + Hit Catch 96b11/32 -0.2 1 0 N 0
  + Hit Catch 96b3/8 0 1 0 N 0
  + Hit Catch 96b13/32 -0.05 1 0 N 0
  + Hit Catch 96b7/16 0.15 1 0 N 0
  + Hit Catch 96b15/32 0.1 1 0 N 0
  + Hit Catch 96b1/2 -0.2 1 0 N 0
  + Hit Catch 96b17/32 -0.25 1 0 N 0
  + Hit Catch 96b9/16 -0.05 1 0 N 0
  + Hit Catch 96b19/32 -0.1 1 0 N 0
  + Hit Catch 96b5/8 0.1 1 0 N 0
  + Hit Catch 96b21/32 0.05 1 0 N 0
  + Hit Catch 96b11/16 0.25 1 0 N 0
  + Hit Catch 96b23/32 0.2 1 0 N 0
  + Hit Catch 96b3/4 -0.1 1 0 N 0
  + Hit Catch 96b25/32 -0.15 1 0 N 0
  + Hit Catch 96b13/16 0.05 1 0 N 0
  + Hit Catch 96b27/32 0 1 0 N 0
  + Hit Catch 96b7/8 0.2 1 0 N 0
  + Hit Catch 96b29/32 0.15 1 0 N 0
  + Hit Catch 96b15/16 0.35 1 0 N 0
  + Hit Catch 96b31/32 0.3 1 0 N 0
  + Hit Catch 97b -0.4 1 0 N 0
  + Hit Catch 97b1/32 -0.45 1 0 N 0
  + Hit Catch 97b1/16 -0.25 1 0 N 0
  + Hit Catch 97b3/32 -0.3 1 0 N 0
  + Hit Catch 97b1/8 -0.1 1 0 N 0
  + Hit Catch 97b5/32 -0.15 1 0 N 0
  + Hit Catch 97b3/16 0.05 1 0 N 0
  + Hit Catch 97b7/32 0 1 0 N 0
  + Hit Catch 97b1/4 -0.3 1 0 N 0
  + Hit Catch 97b9/32 -0.35 1 0 N 0
  + Hit Catch 97b5/16 -0.15 1 0 N 0
  + Hit Catch 97b11/32 -0.2 1 0 N 0
  + Hit Catch 97b3/8 0 1 0 N 0
  + Hit Catch 97b13/32 -0.05 1 0 N 0
  + Hit Catch 97b7/16 0.15 1 0 N 0
  + Hit Catch 97b15/32 0.1 1 0 N 0
  + Hit Catch 97b1/2 -0.2 1 0 N 0
  + Hit Catch 97b17/32 -0.25 1 0 N 0
  + Hit Catch 97b9/16 -0.05 1 0 N 0
  + Hit Catch 97b19/32 -0.1 1 0 N 0
  + Hit Catch 97b5/8 0.1 1 0 N 0
  + Hit Catch 97b21/32 0.05 1 0 N 0
  + Hit Catch 97b11/16 0.25 1 0 N 0
  + Hit Catch 97b23/32 0.2 1 0 N 0
  + Hit Catch 97b3/4 -0.1 1 0 N 0
  + Hit Catch 97b25/32 -0.15 1 0 N 0
  + Hit Catch 97b13/16 0.05 1 0 N 0
  + Hit Catch 97b27/32 0 1 0 N 0
  + Hit Catch 97b7/8 0.2 1 0 N 0
  + Hit Catch 97b29/32 0.15 1 0 N 0
  + Hit Catch 97b15/16 0.35 1 0 N 0
  + Hit Catch 97b31/32 0.3 1 0 N 0
  + Hit Catch 98b -0.4 1 0 N 0
  + Hit Catch 98b1/32 -0.45 1 0 N 0
  + Hit Catch 98b1/16 -0.25 1 0 N 0
  + Hit Catch 98b3/32 -0.3 1 0 N 0
  + Hit Catch 98b1/8 -0.1 1 0 N 0
  + Hit Catch 98b5/32 -0.15 1 0 N 0
  + Hit Catch 98b3/16 0.05 1 0 N 0
  + Hit Catch 98b7/32 0 1 0 N 0
  + Hit Catch 98b1/4 -0.3 1 0 N 0
  + Hit Catch 98b9/32 -0.35 1 0 N 0
  + Hit Catch 98b5/16 -0.15 1 0 N 0
  + Hit Catch 98b11/32 -0.2 1 0 N 0
  + Hit Catch 98b3/8 0 1 0 N 0
  + Hit Catch 98b13/32 -0.05 1 0 N 0
  + Hit Catch 98b7/16 0.15 1 0 N 0
  + Hit Catch 98b15/32 0.1 1 0 N 0
  + Hit Catch 98b1/2 -0.2 1 0 N 0
  + Hit Catch 98b17/32 -0.25 1 0 N 0
  + Hit Catch 98b9/16 -0.05 1 0 N 0
  + Hit Catch 98b19/32 -0.1 1 0 N 0
  + Hit Catch 98b5/8 0.1 1 0 N 0
  + Hit Catch 98b21/32 0.05 1 0 N 0
  + Hit Catch 98b11/16 0.25 1 0 N 0
  + Hit Catch 98b23/32 0.2 1 0 N 0
  + Hit Catch 98b3/4 -0.1 1 0 N 0
  + Hit Catch 98b25/32 -0.15 1 0 N 0
  + Hit Catch 98b13/16 0.05 1 0 N 0
  + Hit Catch 98b27/32 0 1 0 N 0
  + Hit Catch 98b7/8 0.2 1 0 N 0
  + Hit Catch 98b29/32 0.15 1 0 N 0
  + Hit Catch 98b15/16 0.35 1 0 N 0
  + Hit Catch 98b31/32 0.3 1 0 N 0
  + Hit Catch 99b -0.4 1 0 N 0
  + Hit Catch 99b1/32 -0.45 1 0 N 0
  + Hit Catch 99b1/16 -0.25 1 0 N 0
  + Hit Catch 99b3/32 -0.3 1 0 N 0
  + Hit Catch 99b1/8 -0.1 1 0 N 0
  + Hit Catch 99b5/32 -0.15 1 0 N 0
  + Hit Catch 99b3/16 0.05 1 0 N 0
  + Hit Catch 99b7/32 0 1 0 N 0
  + Hit Catch 99b1/4 -0.3 1 0 N 0
  + Hit Catch 99b9/32 -0.35 1 0 N 0
  + Hit Catch 99b5/16 -0.15 1 0 N 0
  + Hit Catch 99b11/32 -0.2 1 0 N 0
  + Hit Catch 99b3/8 0 1 0 N 0
  + Hit Catch 99b13/32 -0.05 1 0 N 0
  + Hit Catch 99b7/16 0.15 1 0 N 0
  + Hit Catch 99b15/32 0.1 1 0 N 0
  + Hit Catch 99b1/2 -0.2 1 0 N 0
  + Hit Catch 99b17/32 -0.25 1 0 N 0
  + Hit Catch 99b9/16 -0.05 1 0 N 0
  + Hit Catch 99b19/32 -0.1 1 0 N 0
  + Hit Catch 99b5/8 0.1 1 0 N 0
  + Hit Catch 99b21/32 0.05 1 0 N 0
  + Hit Catch 99b11/16 0.25 1 0 N 0
  + Hit Catch 99b23/32 0.2 1 0 N 0
  + Hit Catch 99b3/4 -0.1 1 0 N 0
  + Hit Catch 99b25/32 -0.15 1 0 N 0
  + Hit Catch 99b13/16 0.05 1 0 N 0
  + Hit Catch 99b27/32 0 1 0 N 0
  + Hit Catch 99b7/8 0.2 1 0 N 0
  + Hit Catch 99b29/32 0.15 1 0 N 0
  + Hit Catch 99b15/16 0.35 1 0 N 0
  + Hit Catch 99b31/32 0.3 1 0 N 0
  + Hit Catch 100b -0.4 1 0 N 0
  + Hit Catch 100b1/32 -0.45 1 0 N 0
  + Hit Catch 100b1/16 -0.25 1 0 N 0
  + Hit Catch 100b3/32 -0.3 1 0 N 0
  + Hit Catch 100b1/8 -0.1 1 0 N 0
  + Hit Catch 100b5/32 -0.15 1 0 N 0
  + Hit Catch 100b3/16 0.05 1 0 N 0
  + Hit Catch 100b7/32 0 1 0 N 0
  + Hit Catch 100b1/4 -0.3 1 0 N 0
  + Hit Catch 100b9/32 -0.35 1 0 N 0
  + Hit Catch 100b5/16 -0.15 1 0 N 0
  + Hit Catch 100b11/32 -0.2 1 0 N 0
  + Hit Catch 100b3/8 0 1 0 N 0
  + Hit Catch 100b13/32 -0.05 1 0 N 0
  + Hit Catch 100b7/16 0.15 1 0 N 0
  + Hit Catch 100b15/32 0.1 1 0 N 0
  + Hit Catch 100b1/2 -0.2 1 0 N 0
  + Hit Catch 100b17/32 -0.25 1 0 N 0
  + Hit Catch 100b9/16 -0.05 1 0 N 0
  + Hit Catch 100b19/32 -0.1 1 0 N 0
  + Hit Catch 100b5/8 0.1 1 0 N 0
  + Hit Catch 100b21/32 0.05 1 0 N 0
  + Hit Catch 100b11/16 0.25 1 0 N 0
  + Hit Catch 100b23/32 0.2 1 0 N 0
  + Hit Catch 100b3/4 -0.1 1 0 N 0
  + Hit Catch 100b25/32 -0.15 1 0 N 0
  + Hit Catch 100b13/16 0.05 1 0 N 0
  + Hit Catch 100b27/32 0 1 0 N 0
  + Hit Catch 100b7/8 0.2 1 0 N 0
  + Hit Catch 100b29/32 0.15 1 0 N 0
  + Hit Catch 100b15/16 0.35 1 0 N 0
  + Hit Catch 100b31/32 0.3 1 0 N 0
  + Hit Catch 101b -0.4 1 0 N 0
  + Hit Catch 101b1/32 -0.45 1 0 N 0
  + Hit Catch 101b1/16 -0.25 1 0 N 0
  + Hit Catch 101b3/32 -0.3 1 0 N 0
  + Hit Catch 101b1/8 -0.1 1 0 N 0
  + Hit Catch 101b5/32 -0.15 1 0 N 0
  + Hit Catch 101b3/16 0.05 1 0 N 0
  + Hit Catch 101b7/32 0 1 0 N 0
  + Hit Catch 101b1/4 -0.3 1 0 N 0
  + Hit Catch 101b9/32 -0.35 1 0 N 0
  + Hit Catch 101b5/16 -0.15 1 0 N 0
  + Hit Catch 101b11/32 -0.2 1 0 N 0
  + Hit Catch 101b3/8 0 1 0 N 0
  + Hit Catch 101b13/32 -0.05 1 0 N 0
  + Hit Catch 101b7/16 0.15 1 0 N 0
  + Hit Catch 101b15/32 0.1 1 0 N 0
  + Hit Catch 101b1/2 -0.2 1 0 N 0
  + Hit Catch 101b17/32 -0.25 1 0 N 0
  + Hit Catch 101b9/16 -0.05 1 0 N 0
  + Hit Catch 101b19/32 -0.1 1 0 N 0
  + Hit Catch 101b5/8 0.1 1 0 N 0
  + Hit Catch 101b21/32 0.05 1 0 N 0
  + Hit Catch 101b11/16 0.25 1 0 N 0
  + Hit Catch 101b23/32 0.2 1 0 N 0
  + Hit Catch 101b3/4 -0.1 1 0 N 0
  + Hit Catch 101b25/32 -0.15 1 0 N 0
  + Hit Catch 101b13/16 0.05 1 0 N 0
  + Hit Catch 101b27/32 0 1 0 N 0
  + Hit Catch 101b7/8 0.2 1 0 N 0
  + Hit Catch 101b29/32 0.15 1 0 N 0
  + Hit Catch 101b15/16 0.35 1 0 N 0
  + Hit Catch 101b31/32 0.3 1 0 N 0
  + Hit Catch 102b -0.4 1 0 N 0
  + Hit Catch 102b1/32 -0.45 1 0 N 0
  + Hit Catch 102b1/16 -0.25 1 0 N 0
  + Hit Catch 102b3/32 -0.3 1 0 N 0
  + Hit Catch 102b1/8 -0.1 1 0 N 0
  + Hit Catch 102b5/32 -0.15 1 0 N 0
  + Hit Catch 102b3/16 0.05 1 0 N 0
  + Hit Catch 102b7/32 0 1 0 N 0
  + Hit Catch 102b1/4 -0.3 1 0 N 0
  + Hit Catch 102b9/32 -0.35 1 0 N 0
  + Hit Catch 102b5/16 -0.15 1 0 N 0
  + Hit Catch 102b11/32 -0.2 1 0 N 0
  + Hit Catch 102b3/8 0 1 0 N 0
  + Hit Catch 102b13/32 -0.05 1 0 N 0
  + Hit Catch 102b7/16 0.15 1 0 N 0
  + Hit Catch 102b15/32 0.1 1 0 N 0
  + Hit Catch 102b1/2 -0.2 1 0 N 0
  + Hit Catch 102b17/32 -0.25 1 0 N 0
  + Hit Catch 102b9/16 -0.05 1 0 N 0
  + Hit Catch 102b19/32 -0.1 1 0 N 0
  + Hit Catch 102b5/8 0.1 1 0 N 0
  + Hit Catch 102b21/32 0.05 1 0 N 0
  + Hit Catch 102b11/16 0.25 1 0 N 0
  + Hit Catch 102b23/32 0.2 1 0 N 0
  + Hit Catch 102b3/4 -0.1 1 0 N 0
  + Hit Catch 102b25/32 -0.15 1 0 N 0
  + Hit Catch 102b13/16 0.05 1 0 N 0
  + Hit Catch 102b27/32 0 1 0 N 0
  + Hit Catch 102b7/8 0.2 1 0 N 0
  + Hit Catch 102b29/32 0.15 1 0 N 0
  + Hit Catch 102b15/16 0.35 1 0 N 0
  + Hit Catch 102b31/32 0.3 1 0 N 0
  + Hit Catch 103b -0.4 1 0 N 0
  + Hit Catch 103b1/32 -0.45 1 0 N 0
  + Hit Catch 103b1/16 -0.25 1 0 N 0
  + Hit Catch 103b3/32 -0.3 1 0 N 0
  + Hit Catch 103b1/8 -0.1 1 0 N 0
  + Hit Catch 103b5/32 -0.15 1 0 N 0
  + Hit Catch 103b3/16 0.05 1 0 N 0
  + Hit Catch 103b7/32 0 1 0 N 0
  + Hit Catch 103b1/4 -0.3 1 0 N 0
  + Hit Catch 103b9/32 -0.35 1 0 N 0
  + Hit Catch 103b5/16 -0.15 1 0 N 0
  + Hit Catch 103b11/32 -0.2 1 0 N 0
  + Hit Catch 103b3/8 0 1 0 N 0
  + Hit Catch 103b13/32 -0.05 1 0 N 0
  + Hit Catch 103b7/16 0.15 1 0 N 0
  + Hit Catch 103b15/32 0.1 1 0 N 0
  + Hit Catch 103b1/2 -0.2 1 0 N 0
  + Hit Catch 103b17/32 -0.25 1 0 N 0
  + Hit Catch 103b9/16 -0.05 1 0 N 0
  + Hit Catch 103b19/32 -0.1 1 0 N 0
  + Hit Catch 103b5/8 0.1 1 0 N 0
  + Hit Catch 103b21/32 0.05 1 0 N 0
  + Hit Catch 103b11/16 0.25 1 0 N 0
  + Hit Catch 103b23/32 0.2 1 0 N 0
  + Hit Catch 103b3/4 -0.1 1 0 N 0
  + Hit Catch 103b25/32 -0.15 1 0 N 0
  + Hit Catch 103b13/16 0.05 1 0 N 0
  + Hit Catch 103b27/32 0 1 0 N 0
  + Hit Catch 103b7/8 0.2 1 0 N 0
  + Hit Catch 103b29/32 0.15 1 0 N 0
  + Hit Catch 103b15/16 0.35 1 0 N 0
  + Hit Catch 103b31/32 0.3 1 0 N 0
  + Hit Catch 104b -0.4 1 0 N 0
  + Hit Catch 104b1/32 -0.45 1 0 N 0
  + Hit Catch 104b1/16 -0.25 1 0 N 0
  + Hit Catch 104b3/32 -0.3 1 0 N 0
  + Hit Catch 104b1/8 -0.1 1 0 N 0
  + Hit Catch 104b5/32 -0.15 1 0 N 0
  + Hit Catch 104b3/16 0.05 1 0 N 0
  + Hit Catch 104b7/32 0 1 0 N 0
  + Hit Catch 104b1/4 -0.3 1 0 N 0
  + Hit Catch 104b9/32 -0.35 1 0 N 0
  + Hit Catch 104b5/16 -0.15 1 0 N 0
  + Hit Catch 104b11/32 -0.2 1 0 N 0
  + Hit Catch 104b3/8 0 1 0 N 0
  + Hit Catch 104b13/32 -0.05 1 0 N 0
  + Hit Catch 104b7/16 0.15 1 0 N 0
  + Hit Catch 104b15/32 0.1 1 0 N 0
  + Hit Catch 104b1/2 -0.2 1 0 N 0
  + Hit Catch 104b17/32 -0.25 1 0 N 0
  + Hit Catch 104b9/16 -0.05 1 0 N 0
  + Hit Catch 104b19/32 -0.1 1 0 N 0
  + Hit Catch 104b5/8 0.1 1 0 N 0
  + Hit Catch 104b21/32 0.05 1 0 N 0
  + Hit Catch 104b11/16 0.25 1 0 N 0
  + Hit Catch 104b23/32 0.2 1 0 N 0
  + Hit Catch 104b3/4 -0.1 1 0 N 0
  + Hit Catch 104b25/32 -0.15 1 0 N 0
  + Hit Catch 104b13/16 0.05 1 0 N 0
  + Hit Catch 104b27/32 0 1 0 N 0
  + Hit Catch 104b7/8 0.2 1 0 N 0
  + Hit Catch 104b29/32 0.15 1 0 N 0
  + Hit Catch 104b15/16 0.35 1 0 N 0
  + Hit Catch 104b31/32 0.3 1 0 N 0
  + Hit Catch 105b -0.4 1 0 N 0
  + Hit Catch 105b1/32 -0.45 1 0 N 0
  + Hit Catch 105b1/16 -0.25 1 0 N 0
  + Hit Catch 105b3/32 -0.3 1 0 N 0
  + Hit Catch 105b1/8 -0.1 1 0 N 0
  + Hit Catch 105b5/32 -0.15 1 0 N 0
  + Hit Catch 105b3/16 0.05 1 0 N 0
  + Hit Catch 105b7/32 0 1 0 N 0
  + Hit Catch 105b1/4 -0.3 1 0 N 0
  + Hit Catch 105b9/32 -0.35 1 0 N 0
  + Hit Catch 105b5/16 -0.15 1 0 N 0
  + Hit Catch 105b11/32 -0.2 1 0 N 0
  + Hit Catch 105b3/8 0 1 0 N 0
  + Hit Catch 105b13/32 -0.05 1 0 N 0
  + Hit Catch 105b7/16 0.15 1 0 N 0
  + Hit Catch 105b15/32 0.1 1 0 N 0
  + Hit Catch 105b1/2 -0.2 1 0 N 0
  + Hit Catch 105b17/32 -0.25 1 0 N 0
  + Hit Catch 105b9/16 -0.05 1 0 N 0
  + Hit Catch 105b19/32 -0.1 1 0 N 0
  + Hit Catch 105b5/8 0.1 1 0 N 0
  + Hit Catch 105b21/32 0.05 1 0 N 0
  + Hit Catch 105b11/16 0.25 1 0 N 0
  + Hit Catch 105b23/32 0.2 1 0 N 0
  + Hit Catch 105b3/4 -0.1 1 0 N 0
  + Hit Catch 105b25/32 -0.15 1 0 N 0
  + Hit Catch 105b13/16 0.05 1 0 N 0
  + Hit Catch 105b27/32 0 1 0 N 0
  + Hit Catch 105b7/8 0.2 1 0 N 0
  + Hit Catch 105b29/32 0.15 1 0 N 0
  + Hit Catch 105b15/16 0.35 1 0 N 0
  + Hit Catch 105b31/32 0.3 1 0 N 0
  + Hit Catch 106b -0.4 1 0 N 0
  + Hit Catch 106b1/32 -0.45 1 0 N 0
  + Hit Catch 106b1/16 -0.25 1 0 N 0
  + Hit Catch 106b3/32 -0.3 1 0 N 0
  + Hit Catch 106b1/8 -0.1 1 0 N 0
  + Hit Catch 106b5/32 -0.15 1 0 N 0
  + Hit Catch 106b3/16 0.05 1 0 N 0
  + Hit Catch 106b7/32 0 1 0 N 0
  + Hit Catch 106b1/4 -0.3 1 0 N 0
  + Hit Catch 106b9/32 -0.35 1 0 N 0
  + Hit Catch 106b5/16 -0.15 1 0 N 0
  + Hit Catch 106b11/32 -0.2 1 0 N 0
  + Hit Catch 106b3/8 0 1 0 N 0
  + Hit Catch 106b13/32 -0.05 1 0 N 0
  + Hit Catch 106b7/16 0.15 1 0 N 0
  + Hit Catch 106b15/32 0.1 1 0 N 0
  + Hit Catch 106b1/2 -0.2 1 0 N 0
  + Hit Catch 106b17/32 -0.25 1 0 N 0
  + Hit Catch 106b9/16 -0.05 1 0 N 0
  + Hit Catch 106b19/32 -0.1 1 0 N 0
  + Hit Catch 106b5/8 0.1 1 0 N 0
  + Hit Catch 106b21/32 0.05 1 0 N 0
  + Hit Catch 106b11/16 0.25 1 0 N 0
  + Hit Catch 106b23/32 0.2 1 0 N 0
  + Hit Catch 106b3/4 -0.1 1 0 N 0
  + Hit Catch 106b25/32 -0.15 1 0 N 0
  + Hit Catch 106b13/16 0.05 1 0 N 0
  + Hit Catch 106b27/32 0 1 0 N 0
  + Hit Catch 106b7/8 0.2 1 0 N 0
  + Hit Catch 106b29/32 0.15 1 0 N 0
  + Hit Catch 106b15/16 0.35 1 0 N 0
  + Hit Catch 106b31/32 0.3 1 0 N 0
+ Lane 0 1000 0 0 0 0 -1
  $ Offset_Y 91b 0 -4 _ Linear
  + LaneStep 91b -8 0 Linear Linear 8 0 Linear Linear 1
  + LaneStep 92b -8 0 Linear Linear 8 0 Linear Linear 0.2
  + Hit Catch 91b1/3 0.2 0.25 0 N 0
  + Hit Catch 91b1/2 0.55 0.25 0 N 0
  + Hit Catch 91b5/6 0.2 0.25 0 N 0
+ Lane 0 1000 0 0 0 0 0
  $ Offset_Y 107b 0 -4 _ Linear
  $ Offset_Z 107b 1 0 4 Linear
  $ OffsetRotation_X 107b1/2 1 -90 _ Exponential/Out
  + LaneStep 108b -6 0 Linear Linear 6 0 Linear Linear 0
  + LaneStep 110b1/2 -6 0 Linear Linear 6 0 Linear Linear 0.05
  + Hit Normal 108b 0.2 0.3 0 F0 0
  + Hit Normal 108b 0.2 0.3 0 F0 0
  + Hit Normal 108b1/3 0.5 0.3 0 N 0
  + Hit Normal 108b5/6 0.6 0.3 0 N 0
  + Hit Normal 109b 0.1 0.3 0 F0 0
  + Hit Normal 109b1/3 0.6 0.3 0 N 0
  + Hit Normal 109b5/6 0.5 0.3 0 N 0
  + Hit Normal 110b 0.2 0.3 0 F180 0
  + Hit Normal 110b1/3 0.5 0.3 0 N 0
+ Lane 0 1000 0 0 0 0 0
  $ Offset_Y 107b 0 -4 _ Linear
  + LaneStep 108b -8 0 Linear Linear 8 0 Linear Linear 0
  + LaneStep 110b1/2 -8 0 Linear Linear 8 0 Linear Linear 0
  + Hit Catch 108b 0 1 2.5 N 0