# Joints

In the following scripts I will show how to create some different joints through script.

## Distance Joint

You can try this script in an empty map. Just copy-paste it into the script window and test run the map.

```cs
// DISTANCE JOINT TEST
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create two crates - crateA above the ground and the crateB underneath
    IObject crateA = Game.CreateObject("Crate00", new Vector2(20f, 0f), 0f);
    IObject crateB = Game.CreateObject("Crate00", new Vector2(20f, -60f), 0f);

    // Create a distanceJoint which will connect to crateA
    IObjectDistanceJoint distanceJoint = (IObjectDistanceJoint)Game.CreateObject("DistanceJoint");
    distanceJoint.SetWorldPosition(crateA.GetWorldPosition() - new Vector2(0f, 8f));
    distanceJoint.SetTargetObject(crateA);

    // Create a targetObjectJoint which will connect to crateB
    IObjectTargetObjectJoint targetObjectJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint");
    targetObjectJoint.SetWorldPosition(crateB.GetWorldPosition() + new Vector2(0f, 8f));
    targetObjectJoint.SetTargetObject(crateB);

    // Connect the distanceJoint and the targetObjectJoint together
    distanceJoint.SetTargetObjectJoint(targetObjectJoint);

    // Set line visual and type of joint
    distanceJoint.SetLineVisual(LineVisual.DJWire);
    distanceJoint.SetLengthType(DistanceJointLengthType.Elastic); // NOTE: Elastic means the distanceJoint can shrink (but not grow)

    // Set some initial velocity to crateB to make a pendulum
    crateB.SetLinearVelocity(new Vector2(5f, 0f));
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B", new Vector2(8f*(-4.5f), 8f*(-1.5f)), 0f);
    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```

## Weld Joint

You can try this script in an empty map. Just copy-paste it into the script window and test run the map.

```cs
// WELD JOINT TEST
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create some crates in a square-like pattern
    IObject crateA = Game.CreateObject("Crate00", new Vector2(20f, 40f), 0f);
    IObject crateB = Game.CreateObject("Crate00", new Vector2(20f, 0f), 0f);
    IObject crateC = Game.CreateObject("Crate00", new Vector2(-20f, 40f), 0f);
    IObject crateD = Game.CreateObject("Crate00", new Vector2(-20f, 0f), 0f);

    // Create WeldJoint
    IObjectWeldJoint weldJoint = (IObjectWeldJoint)Game.CreateObject("WeldJoint");

    // SetTargetObjects on the weldJoint to weld them together
    weldJoint.SetTargetObjects([crateA, crateB, crateC, crateD]);

    // NOTE: You can also add and remove a single object to the weldJoint with weldJoint.AddTargetObject(IObject) and weldJoint.RemoveTargetObject(IObject)

    // NOTE: Moving an object that's welded to other objects is NOT RECOMMENDED as the box2D will do some average positioning when resolving the weld-joint.
    // crateA.SetWorldPosition(new Vector2(100f, 100f)); // This will not work as intended
    // Instead use the alternative SetWorldPosition that takes a second parameter updateConnectedObjects
    // crateA.SetWorldPosition(new Vector2(100f, 100f), true); // This will work as intended and move all welded objects relatively crateA
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B", new Vector2(8f*(-4.5f), 8f*(-1.5f)), 0f);
    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```

## Elevator Attachment Joint

You can try this script in an empty map. Just copy-paste it into the script window and test run the map.

```cs
// ELEVATOR ATTACHMENT JOINT TEST
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create two elevatorPathJoints and connect them to each other
    IObjectElevatorPathJoint pathA = (IObjectElevatorPathJoint)Game.CreateObject("ElevatorPathJoint", new Vector2(8f*(-3.5f), 8f*(-0.5f)), 0f);
    IObjectElevatorPathJoint pathB = (IObjectElevatorPathJoint)Game.CreateObject("ElevatorPathJoint", new Vector2(8f*(-3.5f), 8f*(8.5f)), 0f);
    pathA.SetNextPathJoint(pathB);
    pathB.SetNextPathJoint(pathA);
    pathA.SetLineVisual(LineVisual.DJRope);

    // Create the elevator joint that will do all the work
    IObjectElevatorAttachmentJoint elevatorJoint = (IObjectElevatorAttachmentJoint)Game.CreateObject("ElevatorAttachmentJoint");
    elevatorJoint.SetWorldPosition(pathA.GetWorldPosition());

    // Create the lift player can stand on
    IObject lift = Game.CreateObject("Lift00A");
    lift.SetWorldPosition(pathA.GetWorldPosition());

    // Connect the elevatorJoint and the lift
    elevatorJoint.SetTargetObject(lift);

    // Connect the elevatorJoint to the path
    elevatorJoint.SetElevatorPathJoint(pathA);

    // NOTE: The elevator musn't overlap or clip anything or it might get stuck!
    // You can not alter collision between objects as of Pre-Alpha 1.7.4 as the AlterCollisionTile is not yet implemented for the ScriptAPI.
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B", new Vector2(8f*(-4.5f), 8f*(-1.5f)), 0f);
    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```

## Pull Joint

You can try this script in an empty map. Just copy-paste it into the script window and test run the map.

```cs
// PULL JOINT TEST
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create two wheels to connect with a pull joint
    IObject wheelA = Game.CreateObject("TruckWheel", new Vector2(10f, 10f));
    IObject wheelB = Game.CreateObject("TruckWheel", new Vector2(10f, -20f));

    // Create the pullJoint for wheelA
    IObjectPullJoint pullJoint = (IObjectPullJoint)Game.CreateObject("PullJoint");
    pullJoint.SetWorldPosition(wheelA.GetWorldPosition());

    // Create the targetObjectJoint for wheelB
    IObjectTargetObjectJoint targetObjectJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint");
    targetObjectJoint.SetWorldPosition(wheelB.GetWorldPosition());

    // Connect targetObjectJoint to wheelB
    targetObjectJoint.SetTargetObject(wheelB);

    // Connect pullJoint to wheelA and targetObjectJoint (which is connected to wheelB)
    pullJoint.SetTargetObject(wheelA);
    pullJoint.SetTargetObjectJoint(targetObjectJoint);

    // Set LineVisual
    pullJoint.SetLineVisual(LineVisual.DJRope);

    // Set forces for the pullJoint
    pullJoint.SetForce(0.6f);
    pullJoint.SetForcePerDistance(0.2f);
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B", new Vector2(8f*(-4.5f), 8f*(-1.5f)), 0f);
    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```

## Rail Joint

You can try this script in an empty map. Just copy-paste it into the script window and test run the map.

```cs
// RAIL JOINT TEST
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create railJoint
    IObjectRailJoint railJoint = (IObjectRailJoint)Game.CreateObject("RailJoint", new Vector2(-40f, 20f), 0f);

    // Create targetObjectJoint (the end of the railJoint)
    IObjectTargetObjectJoint railJointTargetObjectJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", new Vector2(40f, 20f), 0f);

    // Connect the railJoint to the targetObjectJoint
    railJoint.SetTargetObjectJoint(railJointTargetObjectJoint);

    // Enable limits on the rail
    railJoint.SetLimitEnabled(true);

    // Create railAttachmentJoint
    IObjectRailAttachmentJoint railAttachmentJoint = (IObjectRailAttachmentJoint)Game.CreateObject("RailAttachmentJoint");
    railAttachmentJoint.SetWorldPosition(railJoint.GetWorldPosition());

    // Create a pulley
    IObject pulleyObject = Game.CreateObject("Pulley01");
    pulleyObject.SetWorldPosition(railJoint.GetWorldPosition());

    // Connect railAttachmentJoint to the pulley and the railJoint
    railAttachmentJoint.SetTargetObject(pulleyObject);
    railAttachmentJoint.SetRailJoint(railJoint);

    // Set some velocity at startup
    pulleyObject.SetLinearVelocity(new Vector2(1f, 0f));
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B", new Vector2(8f*(-4.5f), 8f*(-1.5f)), 0f);
    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```

## Pulley Joint

You can try this script in an empty map. Just copy-paste it into the script window and test run the map.

```cs
// PULLEY JOINT TEST
public static void OnStartup()
{
    // Create ground
    CreateGround();

    // Create two crates underneath ground
    IObject crateA = Game.CreateObject("Crate00", new Vector2(-20f, -60f));
    IObject crateB = Game.CreateObject("Crate00", new Vector2(20f, -60f));

    // Create pulleyJoint above crateA
    IObjectPulleyJoint pulleyJoint = (IObjectPulleyJoint)Game.CreateObject("PulleyJoint", new Vector2(-20f, 30f));

    // Create pulleyEndJoint above crateB
    IObjectPulleyEndJoint pulleyEndJoint = (IObjectPulleyEndJoint)Game.CreateObject("PulleyEndJoint", new Vector2(20f, 30f));

    // Create targetObjectJoints for both crates
    IObjectTargetObjectJoint targetObjectJointCrateA = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", crateA.GetWorldPosition());
    IObjectTargetObjectJoint targetObjectJointCrateB = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", crateB.GetWorldPosition());
    targetObjectJointCrateA.SetTargetObject(crateA);
    targetObjectJointCrateB.SetTargetObject(crateB);

    // Connect pulleyJoint with crateA (targetObjectJointCrateA)
    pulleyJoint.SetTargetObjectJoint(targetObjectJointCrateA);

    // Connect pulleyEndJoint with crateB (targetObjectJointCrateB)
    pulleyEndJoint.SetTargetObjectJoint(targetObjectJointCrateB);

    // Connect pulleyJoint with pulleyEndJoint
    pulleyJoint.SetPulleyEndJoint(pulleyEndJoint);

    // Set LineVisual
    pulleyJoint.SetLineVisual(LineVisual.DJRope);

    // Set some initial velocity to crateA and crateB
    crateA.SetLinearVelocity(new Vector2(1f, 0.2f));
    crateB.SetLinearVelocity(new Vector2(-0.5f, -0.2f));
}

// Create some ground under the 0,0 coordinate of the world
private static void CreateGround()
{
    IObject ground = Game.CreateObject("Concrete07B", new Vector2(8f*(-4.5f), 8f*(-1.5f)), 0f);
    Point sizeFactor = new(10, 1);
    ground.SetSizeFactor(sizeFactor);
}
```
