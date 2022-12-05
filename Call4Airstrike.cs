using GTA;
using GTA.Math;
using GTA.Native;
using iFruitAddon2;
using System;

public class Call4Airstrike : Script {

    readonly CustomiFruit phone;
    readonly Model bomb_model;

    public Call4Airstrike() {
        Tick += OnTick;

        bomb_model = new Model("prop_ld_bomb_01");

        phone = new CustomiFruit();
        iFruitContact contact = new iFruitContact("Airstrike");
        contact.Answered += Answer; // callback func
        contact.DialTimeout = 2000; // delay before answering
        contact.Active = true; // answers phone
        contact.Icon = ContactIcon.DetonateBomb; // icon
        phone.Contacts.Add(contact);
    }

    private void OnTick(object sender, EventArgs ea) {
        phone?.Update();
    }

    readonly float AirstrikeRadius = 20;
    readonly int AirstrikeBombCount = 100;
    readonly float AirstrikeHeight = 100f;

    readonly Random random = new Random();

    private void Answer(iFruitContact contact) {
        phone?.Close(2000); // 2 seconds

        var waypoint_blip = World.WaypointBlip;

        if (waypoint_blip == null) {
            GTA.UI.Notification.Show("No Airstrike location selected!", false);
        } else {
            var waypoint = waypoint_blip.Position;
            waypoint.Z = World.GetGroundHeight((Vector2) waypoint);

            if (!bomb_model.IsLoaded) {
                if (!bomb_model.Request(1000)) {
                    GTA.UI.Notification.Show("ERROR: Timed out loading bomb model!", false);
                    return;
                }
            }

            GTA.UI.Notification.Show("Airstrike in progress...", true);
            waypoint.Z += AirstrikeHeight;

            for (int i = 0; i < AirstrikeBombCount; i++) {
                var rand_xy = random_vector2d_within_radius(AirstrikeRadius);
                var pos = new Vector3(waypoint.X + (float) rand_xy.Item1,
                                      waypoint.Y + (float) rand_xy.Item2,
                                      waypoint.Z);

                SpawnBomb(pos);
            }
        }
    }

    private (double, double) random_vector2d_within_radius(double radius) {
        var x = random.NextDouble() * radius;
        var y = random.NextDouble() * Math.Sqrt((radius * radius) - (x * x));

        var a = random.NextDouble();
        if (a < .25) { // -, -
            x = -x;
            y = -y;
        } else if (a < .50) { // -, +
            x = -x;
        } else if (a < .75) { // +, -
            y = -y;
        } // else +, +

        return (x, y);
    }

    private void SpawnBomb(Vector3 pos) {
        var bomb_prop = World.CreateProp(bomb_model, pos, false, false);
        Function.Call(Hash.SET_ENTITY_RECORDS_COLLISIONS, bomb_prop.Handle, true);
        Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, bomb_prop.Handle, true);
        Function.Call(Hash.SET_ENTITY_LOD_DIST, bomb_prop.Handle, 10000);
        bomb_prop.Rotation = Vector3.RelativeBottom;
        bomb_prop.Velocity = Vector3.RelativeBottom * 2;
    }

}
