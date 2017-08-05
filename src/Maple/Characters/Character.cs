﻿using Destiny.Network;
using Destiny.Core.IO;
using Destiny.Maple.Maps;
using System;
using Destiny.Core.Network;
using Destiny.Maple.Script;
using System.Collections.Generic;
using Destiny.Maple.Commands;
using Destiny.Maple.Life;
using System.IO;
using Destiny.Data;
using Destiny.Maple.Data;

namespace Destiny.Maple.Characters
{
    public sealed class Character : MapObject, IMoveable, ISpawnable
    {
        public MapleClient Client { get; private set; }

        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public bool IsInitialized { get; private set; }

        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }

        public CharacterItems Items { get; private set; }
        public CharacterSkills Skills { get; private set; }
        public CharacterQuests Quests { get; private set; }
        public ControlledMobs ControlledMobs { get; private set; }
        public ControlledNpcs ControlledNpcs { get; private set; }

        public NpcScript NpcScript { get; set; }

        private DateTime LastHealthHealOverTime = new DateTime();
        private DateTime LastManaHealOverTime = new DateTime();

        private Gender mGender;
        private byte mSkin;
        private int mFace;
        private int mHair;
        private byte mLevel;
        private Job mJob;
        private short mStrength;
        private short mDexterity;
        private short mIntelligence;
        private short mLuck;
        private short mHealth;
        private short mMaxHealth;
        private short mMana;
        private short mMaxMana;
        private short mAbilityPoints;
        private short mSkillPoints;
        private int mExperience;
        private short mFame;
        private int mMeso;

        public Gender Gender
        {
            get
            {
                return MGender;
            }
            set
            {
                MGender = value;

                if (this.IsInitialized)
                {
                    // TODO: Is there a gender set packet?
                }
            }
        }

        public byte Skin
        {
            get
            {
                return MSkin;
            }
            set
            {
                if (!DataProvider.AvailableStyles.Skins.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                MSkin = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Skin);
                    this.UpdateApperance();
                }
            }
        }

        public int Face
        {
            get
            {
                return MFace;
            }
            set
            {
                if ((this.Gender == Gender.Male && !DataProvider.AvailableStyles.MaleFaces.Contains(value)) ||
                    this.Gender == Gender.Female && !DataProvider.AvailableStyles.FemaleFaces.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                MFace = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Face);
                    this.UpdateApperance();
                }
            }
        }

        public int Hair
        {
            get
            {
                return MHair;
            }
            set
            {
                if ((this.Gender == Gender.Male && !DataProvider.AvailableStyles.MaleHairs.Contains(value)) ||
                    this.Gender == Gender.Female && !DataProvider.AvailableStyles.FemaleHairs.Contains(value))
                {
                    throw new StyleUnavailableException();
                }

                MHair = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Hair);
                    this.UpdateApperance();
                }
            }
        }

        public byte Level
        {
            get
            {
                return MLevel;
            }

            set
            {
                MLevel = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Level);

                    using (OutPacket oPacket = new OutPacket(ServerOperationCode.ShowForeignBuff))
                    {
                        oPacket
                            .WriteInt(this.ID)
                            .WriteByte();

                        this.Map.Broadcast(oPacket, this);
                    }
                }
            }
        }

        public Job Job
        {
            get
            {
                return MJob;
            }
            set
            {
                MJob = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Job);

                    using (OutPacket oPacket = new OutPacket(ServerOperationCode.ShowForeignBuff))
                    {
                        oPacket
                            .WriteInt(this.ID)
                            .WriteByte(8);

                        this.Map.Broadcast(oPacket, this);
                    }
                }
            }
        }

        public short Strength
        {
            get
            {
                return MStrength;
            }

            set
            {
                MStrength = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Strength);
                }
            }
        }

        public short Dexterity
        {
            get
            {
                return MDexterity;
            }

            set
            {
                MDexterity = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Dexterity);
                }
            }
        }

        public short Intelligence
        {
            get
            {
                return MIntelligence;
            }

            set
            {
                MIntelligence = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Intelligence);
                }
            }
        }

        public short Luck
        {
            get
            {
                return MLuck;
            }

            set
            {
                MLuck = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Luck);
                }
            }
        }

        public short Health
        {
            get
            {
                return MHealth;
            }

            set
            {
                MHealth = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Health);
                }
            }
        }

        public short MaxHealth
        {
            get
            {
                return MMaxHealth;
            }

            set
            {
                MMaxHealth = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.MaxHealth);
                }
            }
        }

        public short Mana
        {
            get
            {
                return MMana;
            }

            set
            {
                MMana = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Mana);
                }
            }
        }

        public short MaxMana
        {
            get
            {
                return MMaxMana;
            }

            set
            {
                MMaxMana = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.MaxMana);
                }
            }
        }

        public short AbilityPoints
        {
            get
            {
                return MAbilityPoints;
            }

            set
            {
                MAbilityPoints = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.AbilityPoints);
                }
            }
        }

        public short SkillPoints
        {
            get
            {
                return MSkillPoints;
            }

            set
            {
                MSkillPoints = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.SkillPoints);
                }
            }
        }

        public int Experience
        {
            get
            {
                return MExperience;
            }

            set
            {
                MExperience = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Experience);
                }
            }
        }

        public short Fame
        {
            get
            {
                return MFame;
            }

            set
            {
                MFame = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Fame);
                }
            }
        }

        public int Meso
        {
            get
            {
                return MMeso;
            }
            set
            {
                MMeso = value;

                if (this.IsInitialized)
                {
                    this.Update(StatisticType.Mesos);
                }
            }
        }

        public bool IsGm
        {
            get
            {
                //TODO: Add GM levels and/or character-specific GM rank
                return Client.Account != null ? Client.Account.IsMaster : false;
            }
        }

        public bool FacesLeft
        {
            get
            {
                return this.Stance % 2 == 0;
            }
        }

        public bool IsRanked
        {
            get
            {
                return this.Level >= 30;
            }
        }

        private bool Assigned { get; set; }

        public Gender MGender
        {
            get
            {
                return this.mGender;
            }

            set
            {
                this.mGender = value;
            }
        }

        public byte MSkin
        {
            get
            {
                return this.mSkin;
            }

            set
            {
                this.mSkin = value;
            }
        }

        public int MFace
        {
            get
            {
                return this.mFace;
            }

            set
            {
                this.mFace = value;
            }
        }

        public int MHair
        {
            get
            {
                return this.mHair;
            }

            set
            {
                this.mHair = value;
            }
        }

        public byte MLevel
        {
            get
            {
                return this.mLevel;
            }

            set
            {
                this.mLevel = value;
            }
        }

        public Job MJob
        {
            get
            {
                return this.mJob;
            }

            set
            {
                this.mJob = value;
            }
        }

        public short MStrength
        {
            get
            {
                return this.mStrength;
            }

            set
            {
                this.mStrength = value;
            }
        }

        public short MDexterity
        {
            get
            {
                return this.mDexterity;
            }

            set
            {
                this.mDexterity = value;
            }
        }

        public short MIntelligence
        {
            get
            {
                return this.mIntelligence;
            }

            set
            {
                this.mIntelligence = value;
            }
        }

        public short MLuck
        {
            get
            {
                return this.mLuck;
            }

            set
            {
                this.mLuck = value;
            }
        }

        public short MHealth
        {
            get
            {
                return this.mHealth;
            }

            set
            {
                this.mHealth = value;
            }
        }

        public short MMaxHealth
        {
            get
            {
                return this.mMaxHealth;
            }

            set
            {
                this.mMaxHealth = value;
            }
        }

        public short MMana
        {
            get
            {
                return this.mMana;
            }

            set
            {
                this.mMana = value;
            }
        }

        public short MMaxMana
        {
            get
            {
                return this.mMaxMana;
            }

            set
            {
                this.mMaxMana = value;
            }
        }

        public short MAbilityPoints
        {
            get
            {
                return this.mAbilityPoints;
            }

            set
            {
                this.mAbilityPoints = value;
            }
        }

        public short MSkillPoints
        {
            get
            {
                return this.mSkillPoints;
            }

            set
            {
                this.mSkillPoints = value;
            }
        }

        public int MExperience
        {
            get
            {
                return this.mExperience;
            }

            set
            {
                this.mExperience = value;
            }
        }

        public short MFame
        {
            get
            {
                return this.mFame;
            }

            set
            {
                this.mFame = value;
            }
        }

        public int MMeso
        {
            get
            {
                return this.mMeso;
            }

            set
            {
                this.mMeso = value;
            }
        }

        public Character(int id = 0, MapleClient client = null)
        {
            this.ID = id;
            this.Client = client;

            this.Items = new CharacterItems(this, 24, 24, 24, 24, 48);
            this.Skills = new CharacterSkills(this);
            this.Quests = new CharacterQuests(this);

            this.Position = new Point(0, 0);
            this.ControlledMobs = new ControlledMobs(this);
            this.ControlledNpcs = new ControlledNpcs(this);
        }

        public void Load()
        {
            Datum datum = new Datum("characters");

            datum.Populate("ID = '{0}'", this.ID);

            this.ID = (int)datum["ID"];
            this.Assigned = true;

            this.AccountID = (int)datum["AccountID"];
            this.Name = (string)datum["Name"];
            this.Gender = (Gender)datum["Gender"];
            this.Skin = (byte)datum["Skin"];
            this.Face = (int)datum["Face"];
            this.Hair = (int)datum["Hair"];
            this.Level = (byte)datum["Level"];
            this.Job = (Job)datum["Job"];
            this.Strength = (short)datum["Strength"];
            this.Dexterity = (short)datum["Dexterity"];
            this.Intelligence = (short)datum["Intelligence"];
            this.Luck = (short)datum["Luck"];
            this.Health = (short)datum["Health"];
            this.MaxHealth = (short)datum["MaxHealth"];
            this.Mana = (short)datum["Mana"];
            this.MaxMana = (short)datum["MaxMana"];
            this.AbilityPoints = (short)datum["AbilityPoints"];
            this.SkillPoints = (short)datum["SkillPoints"];
            this.Experience = (int)datum["Experience"];
            this.Fame = (short)datum["Fame"];
            this.Map = MasterServer.Channels[this.Client.Channel].Maps[(int)datum["Map"]];
            this.SpawnPoint = (byte)datum["SpawnPoint"];
            this.Meso = (int)datum["Meso"];

            this.Items.MaxSlots[ItemType.Equipment] = (byte)datum["EquipmentSlots"];
            this.Items.MaxSlots[ItemType.Usable] = (byte)datum["UsableSlots"];
            this.Items.MaxSlots[ItemType.Setup] = (byte)datum["SetupSlots"];
            this.Items.MaxSlots[ItemType.Etcetera] = (byte)datum["EtceteraSlots"];
            this.Items.MaxSlots[ItemType.Cash] = (byte)datum["CashSlots"];

            this.Items.Load();
            this.Skills.Load();
            this.Quests.Load();
        }

        public void Save()
        {
            Datum datum = new Datum("characters");

            datum["AccountID"] = this.AccountID;
            datum["Name"] = this.Name;
            datum["Gender"] = (byte)this.Gender;
            datum["Skin"] = this.Skin;
            datum["Face"] = this.Face;
            datum["Hair"] = this.Hair;
            datum["Level"] = this.Level;
            datum["Job"] = (short)this.Job;
            datum["Strength"] = this.Strength;
            datum["Dexterity"] = this.Dexterity;
            datum["Intelligence"] = this.Intelligence;
            datum["Luck"] = this.Luck;
            datum["Health"] = this.Health;
            datum["MaxHealth"] = this.MaxHealth;
            datum["Mana"] = this.Mana;
            datum["MaxMana"] = this.MaxMana;
            datum["AbilityPoints"] = this.AbilityPoints;
            datum["SkillPoints"] = this.SkillPoints;
            datum["Experience"] = this.Experience;
            datum["Fame"] = this.Fame;
            datum["Map"] = this.Map.MapleID;
            datum["SpawnPoint"] = this.SpawnPoint;
            datum["Meso"] = this.Meso;

            datum["EquipmentSlots"] = this.Items.MaxSlots[ItemType.Equipment];
            datum["UsableSlots"] = this.Items.MaxSlots[ItemType.Usable];
            datum["SetupSlots"] = this.Items.MaxSlots[ItemType.Setup];
            datum["EtceteraSlots"] = this.Items.MaxSlots[ItemType.Etcetera];
            datum["CashSlots"] = this.Items.MaxSlots[ItemType.Cash];

            if (this.Assigned)
            {
                datum.Update("ID = '{0}'", this.ID);
            }
            else
            {
                this.ID = datum.InsertAndReturnID();
                this.Assigned = true;
            }

            this.Items.Save();
            this.Skills.Save();
            this.Quests.Save();

            Log.Inform("Saved character '{0}' to database.", this.Name);
        }

        public void Delete()
        {

        }

        public void Initialize(bool cashShop = false)
        {
            using (OutPacket oPacket = new OutPacket(cashShop ? ServerOperationCode.SetCashShop : ServerOperationCode.SetField))
            {
                if (cashShop)
                {
                    this.EncodeData(oPacket);

                    oPacket
                        .WriteByte(1)
                        .WriteMapleString(this.Client.Account.Username)
                        .WriteInt()
                        .WriteShort()
                        .WriteZero(121);

                    for (int i = 1; i <= 8; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            oPacket
                                .WriteInt(i)
                                .WriteInt(j)
                                .WriteInt(50200004)
                                .WriteInt(i)
                                .WriteInt(j)
                                .WriteInt(50200069)
                                .WriteInt(i)
                                .WriteInt(j)
                                .WriteInt(50200117)
                                .WriteInt(i)
                                .WriteInt(j)
                                .WriteInt(50100008)
                                .WriteInt(i)
                                .WriteInt(j)
                                .WriteInt(50000047);
                        }
                    }

                    oPacket
                        .WriteInt()
                        .WriteShort()
                        .WriteByte()
                        .WriteInt(75);
                }
                else
                {
                    oPacket
                        .WriteInt(this.Client.Channel)
                        .WriteByte(++this.Portals)
                        .WriteBool(true)
                        .WriteShort(); // NOTE: Floating messages at top corner.

                    for (int i = 0; i < 3; i++)
                    {
                        oPacket.WriteInt(Constants.Random.Next());
                    }

                    this.EncodeData(oPacket);

                    oPacket.WriteDateTime(DateTime.Now);
                }

                this.Client.Send(oPacket);
            }

            this.IsInitialized = true;

            if (!cashShop)
            {
                this.Map.Characters.Add(this);
            }
        }

        public void Update(params StatisticType[] statistics)
        {
            using (OutPacket oPacket = new OutPacket(ServerOperationCode.StatChanged))
            {
                oPacket.WriteBool(); // TODO: bOnExclRequest.

                int flag = 0;

                foreach (StatisticType statistic in statistics)
                {
                    flag |= (int)statistic;
                }

                oPacket.WriteInt(flag);

                Array.Sort(statistics);

                foreach (StatisticType statistic in statistics)
                {
                    switch (statistic)
                    {
                        case StatisticType.Skin:
                            oPacket.WriteByte(this.Skin);
                            break;

                        case StatisticType.Face:
                            oPacket.WriteInt(this.Face);
                            break;

                        case StatisticType.Hair:
                            oPacket.WriteInt(this.Hair);
                            break;

                        case StatisticType.Level:
                            oPacket.WriteByte(this.Level);
                            break;

                        case StatisticType.Job:
                            oPacket.WriteShort((short)this.Job);
                            break;

                        case StatisticType.Strength:
                            oPacket.WriteShort(this.Strength);
                            break;

                        case StatisticType.Dexterity:
                            oPacket.WriteShort(this.Dexterity);
                            break;

                        case StatisticType.Intelligence:
                            oPacket.WriteShort(this.Intelligence);
                            break;

                        case StatisticType.Luck:
                            oPacket.WriteShort(this.Luck);
                            break;

                        case StatisticType.Health:
                            oPacket.WriteShort(this.Health);
                            break;

                        case StatisticType.MaxHealth:
                            oPacket.WriteShort(this.MaxHealth);
                            break;

                        case StatisticType.Mana:
                            oPacket.WriteShort(this.Mana);
                            break;

                        case StatisticType.MaxMana:
                            oPacket.WriteShort(this.MaxMana);
                            break;

                        case StatisticType.AbilityPoints:
                            oPacket.WriteShort(this.AbilityPoints);
                            break;

                        case StatisticType.SkillPoints:
                            oPacket.WriteShort(this.SkillPoints);
                            break;

                        case StatisticType.Experience:
                            oPacket.WriteInt(this.Experience);
                            break;

                        case StatisticType.Fame:
                            oPacket.WriteShort(this.Fame);
                            break;

                        case StatisticType.Mesos:
                            oPacket.WriteInt(this.Meso);
                            break;
                    }
                }

                this.Client.Send(oPacket);
            }
        }

        public void UpdateApperance()
        {
            using (OutPacket oPacket = new OutPacket(ServerOperationCode.AvatarModified))
            {
                oPacket
                    .WriteInt(this.ID)
                    .WriteBool(true);

                this.EncodeApperance(oPacket);

                oPacket
                    .WriteByte()
                    .WriteShort();

                this.Map.Broadcast(oPacket, this);
            }
        }

        public void Notify(string message, NoticeType type = NoticeType.Pink)
        {
            using (OutPacket oPacket = new OutPacket(ServerOperationCode.BroadcastMsg))
            {
                oPacket.WriteByte((byte)type);

                if (type == NoticeType.Ticker)
                {
                    oPacket.WriteBool(!string.IsNullOrEmpty(message));
                }

                oPacket.WriteMapleString(message);

                this.Client.Send(oPacket);
            }
        }

        public void ChangeMap(InPacket iPacket)
        {
            byte portals = iPacket.ReadByte();

            if (portals != this.Portals)
            {
                return;
            }

            int destinationID = iPacket.ReadInt();

            switch (destinationID)
            {
                case -1:
                    {
                        string label = iPacket.ReadMapleString();
                        Portal portal;

                        try
                        {
                            portal = this.Map.Portals[label];
                            this.ChangeMap(portal.DestinationMap, portal.Link.ID);
                        }
                        catch (KeyNotFoundException)
                        {
                            return;
                        }
                    }
                    break;
            }
        }

        public void ChangeMap(int mapID, byte portalID = 0)
        {
            //If the map doesn't exist, this line will throw an exception. Calling method needs to catch and handle that situation.
            Map newMap = MasterServer.Channels[this.Client.Channel].Maps[mapID];

            this.Map.Characters.Remove(this);

            this.SpawnPoint = portalID;

            using (OutPacket oPacket = new OutPacket(ServerOperationCode.SetField))
            {
                oPacket
                    .WriteInt(this.Client.Channel)
                    .WriteByte(++this.Portals)
                    .WriteBool()
                    .WriteShort()
                    .WriteByte()
                    .WriteInt(mapID)
                    .WriteByte(this.SpawnPoint)
                    .WriteShort(this.Health)
                    .WriteBool(false) // NOTE: Follow.
                    .WriteDateTime(DateTime.Now);

                this.Client.Send(oPacket);
            }

            newMap.Characters.Add(this);
        }

        public void Move(InPacket iPacket)
        {
            Movements movements = Movements.Decode(iPacket);

            // TODO: Validate movements.

            Movement lastMovement = movements[movements.Count - 1];

            this.Position = lastMovement.Position;
            this.Foothold = lastMovement.Foothold;
            this.Stance = lastMovement.Stance;

            using (OutPacket oPacket = new OutPacket(ServerOperationCode.UserMove))
            {
                oPacket.WriteInt(this.ID);

                movements.Encode(oPacket);

                this.Map.Broadcast(oPacket, this);
            }
        }

        public void Sit(InPacket iPacket)
        {
            short seatID = iPacket.ReadShort();

            using (OutPacket oPacket = new OutPacket(ServerOperationCode.Sit))
            {
                oPacket.WriteBool(seatID != -1);

                if (seatID != -1)
                {
                    oPacket.WriteShort(seatID);
                }

                this.Client.Send(oPacket);
            }
        }

        public void Talk(InPacket iPacket)
        {
            string text = iPacket.ReadMapleString();
            bool shout = iPacket.ReadBool(); // NOTE: Used for skill macros.

            if (text.StartsWith(Constants.CommandIndiciator.ToString()))
            {
                CommandFactory.Execute(this, text);
            }
            else
            {
                using (OutPacket oPacket = new OutPacket(ServerOperationCode.UserChat))
                {
                    oPacket
                        .WriteInt(this.ID)
                        .WriteBool(this.IsGm)
                        .WriteMapleString(text)
                        .WriteBool(shout);

                    this.Map.Broadcast(oPacket);
                }
            }
        }

        public void Express(InPacket iPacket)
        {
            int expressionID = iPacket.ReadInt();

            if (expressionID > 7) // NOTE: Cash facial expression.
            {
                int mapleID = 5159992 + expressionID;

                // TODO: Validate if item exists.
            }

            using (OutPacket oPacket = new OutPacket(ServerOperationCode.UserEmotion))
            {
                oPacket
                    .WriteInt(this.ID)
                    .WriteInt(expressionID);

                this.Map.Broadcast(oPacket, this);
            }
        }

        public void Converse(InPacket iPacket)
        {
            int objectID = iPacket.ReadInt();

            this.Converse(this.Map.Npcs[objectID]);
        }

        public void Converse(Npc npc)
        {
            if (this.NpcScript != null)
            {
                return;
            }

            if (!File.Exists(npc.ScriptPath))
            {
                Log.Warn("'{0}' tried to converse with an unimplemented npc {1}.", this.Name, npc.MapleID);
            }
            else
            {
                this.NpcScript = new NpcScript(npc, this);

                try
                {
                    this.NpcScript.Execute();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    this.NpcScript = null;
                }
            }
        }

        public void DistributeAP(StatisticType type, short amount = 1)
        {
            switch (type)
            {
                case StatisticType.Strength:
                    this.Strength += amount;
                    break;

                case StatisticType.Dexterity:
                    this.Dexterity += amount;
                    break;

                case StatisticType.Intelligence:
                    this.Intelligence += amount;
                    break;

                case StatisticType.Luck:
                    this.Luck += amount;
                    break;

                case StatisticType.MaxHealth:
                    // TODO: Get addition based on other factors.
                    break;

                case StatisticType.MaxMana:
                    // TODO: Get addition based on other factors.
                    break;
            }
        }

        public void DistributeAP(InPacket iPacket)
        {
            if (this.AbilityPoints == 0)
            {
                return;
            }

            iPacket.ReadInt(); // NOTE: Ticks.
            StatisticType type = (StatisticType)iPacket.ReadInt();

            this.DistributeAP(type);
            this.AbilityPoints--;
        }

        public void AutoDistributeAP(InPacket iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            int count = iPacket.ReadInt(); // NOTE: There are always 2 primary stats for each job, but still.

            int total = 0;

            for (int i = 0; i < count; i++)
            {
                StatisticType type = (StatisticType)iPacket.ReadInt();
                int amount = iPacket.ReadInt();

                if (amount > this.AbilityPoints || amount < 0)
                {
                    return;
                }

                this.DistributeAP(type, (short)amount);

                total += amount;
            }

            this.AbilityPoints -= (short)total;
        }

        public void HealOverTime(InPacket iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            iPacket.ReadInt(); // NOTE: Unknown.
            short healthAmount = iPacket.ReadShort();
            short manaAmount = iPacket.ReadShort();

            if (healthAmount != 0)
            {
                if ((DateTime.Now - this.LastHealthHealOverTime).Seconds < 2)
                {
                    return;
                }
                else
                {
                    this.Health += healthAmount;
                    this.LastHealthHealOverTime = DateTime.Now;
                }
            }

            if (manaAmount != 0)
            {
                if ((DateTime.Now - this.LastManaHealOverTime).Seconds < 2)
                {
                    return;
                }
                else
                {
                    this.Mana += manaAmount;
                    this.LastManaHealOverTime = DateTime.Now;
                }
            }
        }

        public void DropMeso(InPacket iPacket)
        {
            iPacket.Skip(4); // NOTE: tRequestTime (ticks).
            int amount = iPacket.ReadInt();

            if (amount > this.Meso || amount < 10 || amount > 50000)
            {
                return;
            }

            this.Meso -= amount;

            Meso meso = new Meso(amount)
            {
                Dropper = this,
                Owner = null
            };

            this.Map.Drops.Add(meso);
        }

        public void InformOnCharacter(InPacket iPacket)
        {
            iPacket.Skip(4);
            int characterID = iPacket.ReadInt();

            Character target;

            try
            {
                target = this.Map.Characters[characterID];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            if (target.IsGm && !this.IsGm)
            {
                return;
            }

            using (OutPacket oPacket = new OutPacket(ServerOperationCode.CharacterInformation))
            {
                oPacket
                    .WriteInt(target.ID)
                    .WriteByte(target.Level)
                    .WriteShort((short)target.Job)
                    .WriteShort(target.Fame)
                    .WriteBool() // NOTE: Marriage.
                    .WriteMapleString("-") // NOTE: Guild name.
                    .WriteMapleString("-") // NOTE: Alliance name.
                    .WriteByte() // NOTE: Unknown.
                    .WriteByte() // NOTE: Pets.
                    .WriteByte() // NOTE: Mount.
                    .WriteByte() // NOTE: Wishlist.
                    .WriteInt() // NOTE: Monster Book level.
                    .WriteInt() // NOTE: Monster Book normal cards. 
                    .WriteInt() // NOTE: Monster Book special cards.
                    .WriteInt() // NOTE: Monster Book total cards.
                    .WriteInt() // NOTE: Monster Book cover.
                    .WriteInt() // NOTE: Medal ID.
                    .WriteShort(); // NOTE: Medal quests.

                this.Client.Send(oPacket);
            }
        }

        public void ChangeMapSpecial(InPacket iPacket)
        {
            byte portals = iPacket.ReadByte();

            if (portals != this.Portals)
            {
                return;
            }

            string label = iPacket.ReadMapleString();
            Portal portal;

            try
            {
                portal = this.Map.Portals[label];
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            if (!File.Exists(portal.ScriptPath))
            {
                Log.Warn("'{0}' tried to enter an unimplemented portal '{1}'.", this.Name, portal.Script);
            }
            else
            {
                try
                {
                    new PortalScript(portal, this).Execute();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        public void Encode(OutPacket oPacket)
        {
            this.EncodeStatistics(oPacket);
            this.EncodeApperance(oPacket);

            oPacket
                .WriteByte()
                .WriteBool(this.IsRanked);

            if (this.IsRanked)
            {
                oPacket
                    .WriteInt()
                    .WriteInt()
                    .WriteInt()
                    .WriteInt();
            }
        }

        public void EncodeStatistics(OutPacket oPacket)
        {
            oPacket
                .WriteInt(this.ID)
                .WritePaddedString(this.Name, 13)
                .WriteByte((byte)this.Gender)
                .WriteByte(this.Skin)
                .WriteInt(this.Face)
                .WriteInt(this.Hair)
                .WriteLong()
                .WriteLong()
                .WriteLong()
                .WriteByte(this.Level)
                .WriteShort((short)this.Job)
                .WriteShort(this.Strength)
                .WriteShort(this.Dexterity)
                .WriteShort(this.Intelligence)
                .WriteShort(this.Luck)
                .WriteShort(this.Health)
                .WriteShort(this.MaxHealth)
                .WriteShort(this.Mana)
                .WriteShort(this.MaxMana)
                .WriteShort(this.AbilityPoints)
                .WriteShort(this.SkillPoints)
                .WriteInt(this.Experience)
                .WriteShort(this.Fame)
                .WriteInt()
                .WriteInt(this.Map.MapleID)
                .WriteByte(this.SpawnPoint)
                .WriteInt();
        }

        public void EncodeApperance(OutPacket oPacket)
        {
            oPacket
                .WriteByte((byte)this.Gender)
                .WriteByte(this.Skin)
                .WriteInt(this.Face)
                .WriteBool(true)
                .WriteInt(this.Hair);

            Dictionary<byte, int> visibleLayer = new Dictionary<byte, int>();
            Dictionary<byte, int> hiddenLayer = new Dictionary<byte, int>();

            foreach (Item item in this.Items.GetEquipped())
            {
                byte slot = item.AbsoluteSlot;

                if (slot < 100 && !visibleLayer.ContainsKey(slot))
                {
                    visibleLayer[slot] = item.MapleID;
                }
                else if (slot > 100 && slot != 111)
                {
                    slot -= 100;

                    if (visibleLayer.ContainsKey(slot))
                    {
                        hiddenLayer[slot] = visibleLayer[slot];
                    }

                    visibleLayer[slot] = item.MapleID;
                }
                else if (visibleLayer.ContainsKey(slot))
                {
                    hiddenLayer[slot] = item.MapleID;
                }
            }

            foreach (KeyValuePair<byte, int> entry in visibleLayer)
            {
                oPacket
                    .WriteByte(entry.Key)
                    .WriteInt(entry.Value);
            }

            oPacket.WriteByte(byte.MaxValue);

            foreach (KeyValuePair<byte, int> entry in hiddenLayer)
            {
                oPacket
                    .WriteByte(entry.Key)
                    .WriteInt(entry.Value);
            }

            oPacket.WriteByte(byte.MaxValue);

            Item cashWeapon = this.Items[EquipmentSlot.CashWeapon];

            oPacket.WriteInt(cashWeapon != null ? cashWeapon.MapleID : 0);

            oPacket
                .WriteInt()
                .WriteInt()
                .WriteInt();
        }

        public void EncodeData(OutPacket oPacket, long flag = long.MaxValue)
        {
            oPacket
                .WriteLong(flag)
                .WriteByte(); // NOTE: Unknown.

            this.EncodeStatistics(oPacket);

            oPacket
                .WriteByte(20) // NOTE: Max buddylist size.
                .WriteBool(false) // NOTE: Blessing of Fairy.
                .WriteInt(this.Meso);

            this.Items.Encode(oPacket);
            this.Skills.Encode(oPacket);
            this.Quests.Encode(oPacket);

            oPacket
                .WriteShort() // NOTE: Mini games record.
                .WriteShort() // NOTE: Rings (1).
                .WriteShort() // NOTE: Rings (2). 
                .WriteShort(); // NOTE: Rings (3).

            // NOTE: Teleport rock locations.
            for (int i = 0; i < 15; i++)
            {
                oPacket.WriteInt(999999999);
            }

            oPacket
                .WriteInt() // NOTE: Monster book cover ID.
                .WriteByte() // NOTE: Unknown.
                .WriteShort() // NOTE: Monster book cards count.
                .WriteShort() // NOTE: New year cards.
                .WriteShort() // NOTE: Area information.
                .WriteShort(); // NOTE: Unknown.
        }

        public OutPacket GetCreatePacket()
        {
            return this.GetSpawnPacket();
        }

        public OutPacket GetSpawnPacket()
        {
            OutPacket oPacket = new OutPacket(ServerOperationCode.UserEnterField);

            oPacket
                .WriteInt(this.ID)
                .WriteByte(this.Level)
                .WriteMapleString(this.Name)
                .WriteMapleString(string.Empty) // NOTE: Guild name.
                .WriteZero(6) // NOTE: Guild emblems.
                .WriteInt()
                .WriteShort()
                .WriteByte(0xFC)
                .WriteByte(1)
                .WriteInt();

            int buffmask = 0;

            oPacket
                .WriteInt((int)((buffmask >> 32) & 0xFFFFFFFFL))
                .WriteInt((int)(buffmask & 0xFFFFFFFFL));

            int magic = Constants.Random.Next();

            oPacket
                .WriteZero(6)
                .WriteInt(magic)
                .WriteZero(11)
                .WriteInt(magic)
                .WriteZero(11)
                .WriteInt(magic)
                .WriteShort()
                .WriteByte()
                .WriteLong()
                .WriteInt(magic)
                .WriteZero(9)
                .WriteInt(magic)
                .WriteShort()
                .WriteInt()
                .WriteZero(10)
                .WriteInt(magic)
                .WriteZero(13)
                .WriteInt(magic)
                .WriteShort()
                .WriteByte()
                .WriteShort((short)this.Job);

            this.EncodeApperance(oPacket);

            oPacket
                .WriteInt()
                .WriteInt()
                .WriteInt()
                .WritePoint(this.Position)
                .WriteByte(this.Stance)
                .WriteShort(this.Foothold)
                .WriteByte()
                .WriteByte()
                .WriteInt(1)
                .WriteLong()
                .WriteBool()
                .WriteBool()
                .WriteByte()
                .WriteByte()
                .WriteByte()
                .WriteZero(3)
                .WriteByte(byte.MaxValue);

            return oPacket;
        }

        public OutPacket GetDestroyPacket()
        {
            OutPacket oPacket = new OutPacket(ServerOperationCode.UserLeaveField);

            oPacket.WriteInt(this.ID);

            return oPacket;
        }
    }
}