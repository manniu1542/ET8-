using System.Collections.Generic;

namespace ET
{
    [ComponentOf]
    public class TestScene : Entity, IScene, IAwake
    {
        public Fiber Fiber { get; set; }
        public SceneType SceneType { get; set; }
    }
    
  
    
}