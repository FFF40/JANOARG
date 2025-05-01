JANOARG Chart Format
github.com/ducdat0507/janoarg

[METADATA]
Index: -1
Name: INFORMATION/OVERLOAD
Level: 47*
Constant: 47

[CAMERA]
Pivot: 0 0 0
Rotation: 0 0 0
Distance: 10
$ PivotDistance 34 2 10 30 Cubic/Out
$ CameraRotation_X 34 2 0 45 Cubic/Out
$ CameraRotation_Z 34 2 0 15 Quadratic/Out
$ CameraRotation_Z 36 8 10 _ Quadratic/Out
$ CameraRotation_Z 44 24 -45 _ Quadratic/InOut
$ CameraRotation_Z 68 30 45 _ Quadratic/InOut

[GROUPS]
+ Group 0 0 0 0 0 0
  Name: Background 1
  $ Rotation_Z 4 30 90 _ Sine/InOut

[PALLETE]
Background: 0.04 0.04 0.06 1
Interface: 1 1 1 1
$ BackgroundColor_R 4 16 0.04 0.24 Linear/Out
$ BackgroundColor_G 4 16 0.04 0.26 Linear/Out
$ BackgroundColor_B 4 16 0.06 0.26 Linear/Out
$ BackgroundColor_R 35.99996 8 0.04 0.24 Linear/Out
$ BackgroundColor_G 35.99996 8 0.04 0.26 Linear/Out
$ BackgroundColor_B 35.99996 8 0.06 0.26 Linear/Out
+ LaneStyle 0.127759 0.2735849 0.2563569 0.4 0 0 0 1
+ HitStyle 1 1 1 0.6 1 1 1 1 0.3515486 0.7703948 0.9433962 1

[OBJECTS]
+ Lane 0 0 -10 0 0 0 0
  Group: Background 1
  $ Offset_Y -4 8 0 500 Linear/In
  $ Offset_Y 4 30 60 _ Linear/Out
  $ Offset_Y 34 2 20 _ Exponential/In
  + LaneStep -4 -60 0 Linear/In Linear/In 60 0 Linear/In Linear/In 1
  + LaneStep 96 -60 0 Linear/In Linear/In 60 0 Linear/In Linear/In 1
+ Lane 0 0 -10 0 0 0 0
  Group: Background 1
  $ Offset_Y -4 8 0 -500 Linear/In
  $ Offset_Y 3.999962 30 -60 _ Linear/Out
  $ Offset_Y 34 2 -20 _ Exponential/In
  + LaneStep -4 -60 0 Linear/In Linear/In 60 0 Linear/In Linear/In 1
  + LaneStep 96 -60 0 Linear/In Linear/In 60 0 Linear/In Linear/In 1
+ Lane 0 -4 0 0 0 0 0
  + LaneStep 33.99996 -3 0 Linear/In Linear/In 3 0 Linear/In Linear/In 1
  + LaneStep 99.99996 -3 0 Linear/In Linear/In 3 0 Linear/In Linear/In 1
  + Hit Normal 36 0.05 0.9 8 N 0
+ Lane 3.256724 2.322445 0 0 0 120 0
  + LaneStep 33.99996 -3 0 Linear/In Linear/In 3 0 Linear/In Linear/In 1
  + LaneStep 99.99996 -3 0 Linear/In Linear/In 3 0 Linear/In Linear/In 1
  + Hit Normal 36 0 1 0 N 0
  + Hit Normal 38 0 1 0 N 0
+ Lane -3.256724 2.322445 0 0 0 -120 0
  + LaneStep 34 -3 0 Linear/In Linear/In 3 0 Linear/In Linear/In 1
  + LaneStep 100 -3 0 Linear/In Linear/In 3 0 Linear/In Linear/In 1