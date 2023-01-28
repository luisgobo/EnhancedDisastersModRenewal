using System;
using ICities;
using UnityEngine;


namespace TestMod.Source
{
    public class TestMod : IUserMod
    {
        public string Name => "Test Mod";

        public string Description => "Test Mod to verify entity framework version";
    }
}
