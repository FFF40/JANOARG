JANOARG Chart Format
github.com/ducdat0507/janoarg

[METADATA]
Index: -1
Name: ?????????????????????
Charter: duducat
Level: ??
Constant: 0

[CAMERA]
Pivot: -12 17.5 -12
Rotation: 45 45 0
Distance: 10

[GROUPS]
+ Group 0 0 0 0 0 0
  Name: Cup
  $ Position_Y 0 4 1 _ Sine/InOut
  $ Rotation_Y 0 64 1200 _ Linear/In
  $ Position_Y 4 4 0 _ Sine/InOut
  $ Position_Y 8 4 1 _ Sine/InOut
  $ Position_Y 12 4 0 _ Sine/InOut
  $ Position_Y 16 4 1 _ Sine/InOut
  $ Position_Y 20 4 0 _ Sine/InOut
  $ Position_Y 24 4 1 _ Sine/InOut
  $ Position_Y 28 4 0 _ Sine/InOut
  $ Position_Y 32 4 1 _ Sine/InOut
  $ Position_Y 36 4 0 _ Sine/InOut
  $ Position_Y 40 4 1 _ Sine/InOut
  $ Position_Y 44 4 0 _ Sine/InOut
  $ Position_Y 48 4 1 _ Sine/InOut
  $ Position_Y 52 4 0 _ Sine/InOut
  $ Position_Y 56 4 1 _ Sine/InOut
  $ Position_Y 60 4 0 _ Sine/InOut
+ Group 0 0 0 0 0 0
  Name: Spinner
  $ Rotation_X 0 64 2717 _ Linear/In
  $ Rotation_Y 0 64 3141 _ Linear/In
  $ Rotation_Z 0 64 1337 _ Linear/In

[PALLETE]
Background: 1 1 1 1
Interface: 0 0 0 1
+ LaneStyle 1 1 1 0.6 0 0 0 1
+ HitStyle 0 0 0 1 0 0 0 1 0 0 1 1

[OBJECTS]
+ Lane 0 0 -20 0 180 0 0
  Group: Spinner
  + LaneStep 0 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
  + LaneStep 64 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
+ Lane 0 0 20 0 0 0 0
  Group: Spinner
  + LaneStep 0 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
  + LaneStep 64 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
+ Lane 20 0 0 0 90 0 0
  Group: Spinner
  + LaneStep 0 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
  + LaneStep 64 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
+ Lane -20 0 0 0 -90 0 0
  Group: Spinner
  + LaneStep 0 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
  + LaneStep 64 -6 0 Linear/In Linear/In 6 0 Linear/In Linear/In 1
+ Lane 0 -2 0 90 0 0 0
  Group: Cup
  + LaneStep 64 1 -1 Linear/In Linear/In -1 -1 Linear/In Linear/In 0
  + LaneStep 64.001 1 1 Linear/In Linear/In -1 1 Linear/In Linear/In 1
+ Lane 0 -2 -1 -5 0 0 0
  Group: Cup
  + LaneStep 64 1 0 Linear/In Linear/In -1 0 Linear/In Linear/In 0
  + LaneStep 64.001 1.5 6 Linear/In Linear/In -1.5 6 Linear/In Linear/In 1
+ Lane -1 -2 0 -5 -270 0 0
  Group: Cup
  + LaneStep 64 1 0 Linear/In Linear/In -1 0 Linear/In Linear/In 0
  + LaneStep 64.001 1.5 6 Linear/In Linear/In -1.5 6 Linear/In Linear/In 1
+ Lane 1 -2 0 -5 -90 0 0
  Group: Cup
  + LaneStep 64 1 0 Linear/In Linear/In -1 0 Linear/In Linear/In 0
  + LaneStep 64.001 1.5 6 Linear/In Linear/In -1.5 6 Linear/In Linear/In 1
+ Lane 0 -2 1 -5 -180 0 0
  Group: Cup
  + LaneStep 64 1 0 Linear/In Linear/In -1 0 Linear/In Linear/In 0
  + LaneStep 64.001 1.5 6 Linear/In Linear/In -1.5 6 Linear/In Linear/In 1
+ Lane 0.95 2 1 0 0 75 0
  Group: Cup
  + LaneStep 64.00003 4 0 Linear/In Linear/In -4 0 Linear/In Linear/In 0
  + Hit Normal 64 0 1 0 N 0