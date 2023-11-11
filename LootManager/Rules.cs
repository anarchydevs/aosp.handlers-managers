﻿namespace LootManager
{
    public class Rule
    {
        public string Name = "";
        public string Lql = "";
        public string Hql = "";
        public bool Global = true;
        public string Quantity = "";
        public string BagName = "";

        //public Rule(string name, string lql, string hql, bool global)
        //{
        //    this.Name = name;
        //    this.Lql = lql;
        //    this.Hql = hql;
        //    this.Global = global;
        //}

        public Rule(string name, string lql, string hql, bool global, string quantity, string bagName)
        {
            this.Name = name;
            this.Lql = lql;
            this.Hql = hql;
            this.Global = global;
            this.Quantity = quantity;
            this.BagName = bagName;
        }
    }
}
