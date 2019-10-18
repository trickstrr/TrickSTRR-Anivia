namespace TrickSTRRAnivia
{
    using System;
    using System.Linq;
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
    using EnsoulSharp.SDK.Prediction;
    using EnsoulSharp.SDK.Utility;
    using Color = System.Drawing.Color;

    public class Program
    {
        private static Menu MainMenu;

        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;

        private static void Main(string[] args)
        {

            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {

            if (ObjectManager.Player.CharacterName != "Anivia")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1075);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 750);

            Q.SetSkillshot(.5f, 110f, 750f, false, SkillshotType.Line);
            W.SetSkillshot(.25f, 1f, float.MaxValue, false, SkillshotType.Line);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(.25f, 200f, float.MaxValue, false, SkillshotType.Circle);


            MainMenu = new Menu("trickstrranivia", "TrickSTRR Anivia", true);


            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.Add(new MenuBool("comboQ", "Use Q", true));
            comboMenu.Add(new MenuBool("comboW", "Use W", true));
            comboMenu.Add(new MenuBool("comboR", "Use R", true));
            MainMenu.Add(comboMenu);

            var laneclearMenu = new Menu("Clear", "Lane Clear");
            laneclearMenu.Add(new MenuBool("clearR", "Use R", true));
            MainMenu.Add(laneclearMenu);

            var harassMenu = new Menu("Harass", "Harass");
            harassMenu.Add(new MenuBool("harassQ", "Use Q", true));
            harassMenu.Add(new MenuBool("harassR", "Use R", true));
            harassMenu.Add(new MenuSlider("ManaHarass", "Harass ManaPercent", 60, 0, 100));
            MainMenu.Add(harassMenu);

            // Skinchanger doesnt Work, need to fix. 
            var miscMenu = new Menu("Misc", "Misc");

            miscMenu.Add(new MenuBool("qagapclose", "Use Q to AntiGapclosers"));
            miscMenu.Add(new MenuBool("skinHack", "Skin Change"));
            miscMenu.Add(new MenuSlider("SkinID", "Skin", 0, 0, 9));
            MainMenu.Add(miscMenu);

            var drawMenu = new Menu("Draw", "Draw");
            drawMenu.Add(new MenuBool("qRange", "Q range", false));
            drawMenu.Add(new MenuBool("wRange", "W range", false));
            drawMenu.Add(new MenuBool("eRange", "E range", false));
            drawMenu.Add(new MenuBool("skillReady", "Draw when skill ready", true));


            MainMenu.Attach();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Chat.Print("TrickSTRR Anivia, Please Report Any Problems to my Discord, Thanks! ;-;");

        }

        // Combos goeing successfuly
        private static void Combo()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, DamageType.True);


            if (MainMenu["Combo"]["comboQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                if  
                    ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) /
                    (qTarget.Health / qTarget.MaxHealth) < 1)
                    
                {
                    if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                    {/*
                        var rPrediction = R.GetPrediction(rTarget);
                        if (rPrediction.Hitchance >= HitChance.High)
                        {

                            R.Cast(rPrediction.CastPosition);

                        }*/
                    }

                    if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                    {
                        Q.Cast();
                    }
                }
            }

            if (MainMenu["Combo"]["comboQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    
                       var rPrediction = R.GetPrediction(rTarget);
                    if (rPrediction.Hitchance >= HitChance.High)
                    {
                        R.Cast(rPrediction.CastPosition);
                    }
                    
                }

                if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    var qPrediction = Q.GetPrediction(qTarget);
                    if (qPrediction.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qPrediction.CastPosition);
                    }
                    else if (qPrediction.Hitchance == HitChance.Collision)
                    {
                        var minionsHit = qPrediction.CollisionObjects;
                        var closest =
                            minionsHit.Where(m => m.NetworkId != ObjectManager.Player.NetworkId)
                                .OrderBy(m => m.Distance(ObjectManager.Player))
                                .FirstOrDefault();

                        if (closest != null && closest.Distance(qPrediction.UnitPosition) < 200)
                        {
                            Q.Cast(qPrediction.CastPosition);
                        }
                    }
                }
            }

            if (MainMenu["Combo"]["comboE"].GetValue<MenuBool>().Enabled && qTarget != null)
            {
                E.Cast();
            }

        }
        private static void Clear()
        {

            var rminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange()) && x.IsMinion()).Cast<AIBaseClient>().ToList();

            foreach (AIBaseClient minion in rminions)
            {
                if (R.IsReady())
                {
                    if (minion.Health <= R.GetDamage(minion))
                        R.Cast();
                    // BETA, should improve this @TrickSTRR
                }
            }


        }

        private static void Harass()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (MainMenu["Harass"]["harassQ"].GetValue<MenuBool>().Enabled && qTarget != null && Player.Instance.ManaPercent < MainMenu["Harass"].GetValue<MenuSlider>("ManaHarass").Value)
            {

                if (MainMenu["Harass"]["harassR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    R.Cast();
                }

                if (MainMenu["Harass"]["harassR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    var qPrediction = Q.GetPrediction(qTarget);
                    if (qPrediction.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qPrediction.CastPosition);
                    }

                    else if (qPrediction.Hitchance == HitChance.Collision)
                    {
                        var minionsHit = qPrediction.CollisionObjects;
                        var closest =
                            minionsHit.Where(m => m.NetworkId != ObjectManager.Player.NetworkId)
                                .OrderBy(m => m.Distance(ObjectManager.Player))
                                .FirstOrDefault();

                        if (closest != null && closest.Distance(qPrediction.UnitPosition) < 200)
                        {
                            Q.Cast(qPrediction.CastPosition);
                        }


                    }

                }

                if (MainMenu["Harass"]["harassE"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    E.Cast();
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args)

        {
            if (!MainMenu["Misc"].GetValue<MenuBool>("wgapclose"))
                return;
            var attacker = sender;
            if (attacker.IsValidTarget(300f))
            {
                W.Cast(ObjectManager.Player);

            }

            if (!MainMenu["Misc"].GetValue<MenuBool>("qgapclose"))
                return;
            var attacker1 = sender;
            if (attacker.IsValidTarget(300f))
            {
                Q.Cast(ObjectManager.Player);

            }
        }

      public static void ExecuteAdditionals()
        {    
            if (MainMenu["Misc"]["Eshield"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                foreach (var hero in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(
                            hero =>
                                hero.IsValidTarget(E.Range, false) && hero.IsAlly &&
                                ObjectManager.Get<AIHeroClient>().Count(h => h.IsValidTarget() && h.Distance(hero) < 400) >
                                1))
                {
                    E.Cast(hero); 
                }
            }
        }



      private static void OnUpdate(EventArgs args)
        {
            
            switch (Orbwalker.ActiveMode)
            {    
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
            }
        }

     private static void OnDraw(EventArgs args)
        {

            /*   if (MainMenu["Draw"]["drawQ"].GetValue<MenuBool>().Enabled)
                   {
                       Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Blue);
                   }
                   if (MainMenu["Draw"]["drawW"].GetValue<MenuBool>().Enabled)
                   {
                       Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White);
                   }
                   if (MainMenu["Draw"]["drawE"].GetValue<MenuBool>().Enabled)
                   {
                       Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Cyan);
                   }
                   if (MainMenu["Draw"]["drawR"].GetValue<MenuBool>().Enabled)
                   {
                       Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.DarkCyan);

                   }*/
        }
    }
}