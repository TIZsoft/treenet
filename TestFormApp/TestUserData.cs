namespace TestFormApp
{
    public class TestUserData
    {
        public string guid { get; set; }
        public string fb_id { get; set; }
        public string name { get; set; }
        public int level { get; set; }

        public override string ToString()
        {
            return string.Format("guid={0}, fb_id={1}, name={2}, level={3}", guid, fb_id, name, level);
        }
    }
}