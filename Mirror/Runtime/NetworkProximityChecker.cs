using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    [AddComponentMenu("Network/NetworkProximityChecker")]
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkProximityChecker : NetworkBehaviour
    {
        public enum CheckMethod
        {
            Physics3D,
            Physics2D
        };

        [TooltipAttribute("The maximum range that objects will be visible at.")]
        public int visRange = 10;

        [TooltipAttribute("How often (in seconds) that this object should update the set of players that can see it.")]
        public float visUpdateInterval = 1.0f; // in seconds

        [TooltipAttribute("Which method to use for checking proximity of players.\n\nPhysics3D uses 3D physics to determine proximity.\n\nPhysics2D uses 2D physics to determine proximity.")]
        public CheckMethod checkMethod = CheckMethod.Physics3D;

        [TooltipAttribute("Enable to force this object to be hidden from players.")]
        public bool forceHidden = false;

        float m_VisUpdateTime;

        void Update()
        {
            if (!NetworkServer.active)
                return;

            if (Time.time - m_VisUpdateTime > visUpdateInterval)
            {
                GetComponent<NetworkIdentity>().RebuildObservers(false);
                m_VisUpdateTime = Time.time;
            }
        }

        // called when a new player enters
        public override bool OnCheckObserver(NetworkConnection newObserver)
        {
            if (forceHidden)
                return false;

            // this cant use newObserver.playerControllers[0]. must iterate to find a valid player.
            PlayerController controller = newObserver.playerControllers.Find(
                pc => pc != null && pc.gameObject != null
            );
            if (controller != null)
            {
                GameObject player = controller.gameObject;
                return Vector3.Distance(player.transform.position, transform.position) < visRange;
            }
            return false;
        }

        public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
        {
            if (forceHidden)
            {
                // ensure player can still see themself
                var uv = GetComponent<NetworkIdentity>();
                if (uv.connectionToClient != null)
                {
                    observers.Add(uv.connectionToClient);
                }
                return true;
            }

            // find players within range
            switch (checkMethod)
            {
                case CheckMethod.Physics3D:
                {
                    var hits = Physics.OverlapSphere(transform.position, visRange);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var hit = hits[i];
                        // (if an object has a connectionToClient, it is a player)
                        var uv = hit.GetComponent<NetworkIdentity>();
                        if (uv != null && uv.connectionToClient != null)
                        {
                            observers.Add(uv.connectionToClient);
                        }
                    }
                    return true;
                }

                case CheckMethod.Physics2D:
                {
                    var hits = Physics2D.OverlapCircleAll(transform.position, visRange);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var hit = hits[i];
                        // (if an object has a connectionToClient, it is a player)
                        var uv = hit.GetComponent<NetworkIdentity>();
                        if (uv != null && uv.connectionToClient != null)
                        {
                            observers.Add(uv.connectionToClient);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        // called hiding and showing objects on the host
        public override void OnSetLocalVisibility(bool vis)
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                rend.enabled = vis;
            }
        }
    }
}
