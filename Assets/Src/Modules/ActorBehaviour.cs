using UnityEngine;

namespace LapisPlayer
{
    public class ActorBehaviour : MonoBehaviour
    {
        public int Postion { get; private set; }

        public void BindPosition (int position)
        {
            Postion = position;
        }
    }
}
