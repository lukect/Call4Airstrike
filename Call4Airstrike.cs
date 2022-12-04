using GTA;
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
    readonly int AirstrikeBombCount = 25;
    readonly float AirstrikeHeight = 100f;

    readonly Random random = new Random();

    private void Answer(iFruitContact contact) {
        phone?.Close(2000); // 2 seconds

        var waypoint = World.WaypointPosition;

        if (waypoint == null) {
            GTA.UI.Notification.Show("No Airstrike location selected!", false);
        } else {
            if (!bomb_model.IsLoaded) {
                if (!bomb_model.Request(1000)) {
                    GTA.UI.Notification.Show("ERROR: Timed out loading bomb model!", false);
                    return;
                }
            }

            GTA.UI.Notification.Show("Airstrike in progress...", true);
            waypoint.Z += AirstrikeHeight;

            for (int i = 0; i < AirstrikeBombCount; i++) {
                float angle = (float)(random.NextDouble() * (2 * Math.PI));
                float distance = (float)(random.NextDouble() * AirstrikeRadius);

                var pos = new GTA.Math.Vector3(waypoint.X + ((float)Math.Cos(angle) * distance),
                                               waypoint.Y + ((float)Math.Sin(angle) * distance),
                                               waypoint.Z);

                SpawnBomb(pos);
            }
        }
    }

    private void SpawnBomb(GTA.Math.Vector3 pos) {
        var bomb_prop = World.CreateProp(bomb_model, pos, false, false);
        Function.Call(Hash.SET_ENTITY_RECORDS_COLLISIONS, bomb_prop.Handle, true);
        Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, bomb_prop.Handle, true);
        Function.Call(Hash.SET_ENTITY_LOD_DIST, bomb_prop.Handle, 10000);
        bomb_prop.Rotation = GTA.Math.Vector3.RelativeBottom;
        bomb_prop.Velocity = GTA.Math.Vector3.RelativeBottom * 10;
    }

}
