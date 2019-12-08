using System;
using System.Collections.Generic;
using System.Text;

namespace FTN.Common
{
	
	public enum DMSType : short
	{		
		MASK_TYPE							= unchecked((short)0xFFFF),

        DISCRETE                            = 0x0001,
        ANALOG                              = 0x0002,
        GENERATOR                           = 0x0003,
        SUBSTATION                          = 0x0004,
        GEOGRAFICAL_REGION                  = 0x0005,
        BATTERY_STORAGE                     = 0x0006,

		//BASEVOLTAGE							= 0x0007,
		//LOCATION							= 0x0008,
		//POWERTR								= 0x0009,
		//POWERTRWINDING						= 0x0010,
		//WINDINGTEST							= 0x0011,
	}

    [Flags]
	public enum ModelCode : long
	{
        IDOBJ                                       = 0x1000000000000000,
        IDOBJ_GID                                   = 0x1000000000000104,
        IDOBJ_ALIASNAME                             = 0x1000000000000207,
        IDOBJ_MRID                                  = 0x1000000000000307,
        IDOBJ_NAME                                  = 0x1000000000000407,

        PSR                                         = 0x1100000000000000,
        PSR_MEASUREMENTS                            = 0x1100000000000119,

        CONECTIVITY_NODE_CONTAINER                  = 0x1110000000000000,

        EQUIPMENT_CONTAINER                         = 0x1111000000000000,
        EQUIPMENT_CONTAINER_EQUIPMENTS              = 0x1111000000000119,

        SUBSTATION                                  = 0x1111100000040000,

        EQUIPMENT                                   = 0x1120000000000000,
        EQUIPMENT_EQUIPMENT_CONTAINER               = 0x1120000000000109,

        CONDUCTING_EQUIPMENT                        = 0x1121000000000000,

        REGULATING_CONDUCTING_EQUIPMENT             = 0x1121100000000000,

        ROTATING_MACHINE                            = 0x1121110000000000,
        ROTATING_MACHINE_RATED_S                    = 0x1121110000000105,

        GENERATOR                                   = 0x1121111000030000,
        GENERATOR_MAX_Q                             = 0x1121111000030105,
        GENERATOR_MIN_Q                             = 0x1121111000030205,
        GENERATOR_TYPE                              = 0x112111100003030a,

        ENERGY_CONSUMER                             = 0x1121200000000000,
        ENERGY_CONSUMER_CURRENT_POWER               = 0x1121200000000105,
        ENERGY_CONSUMER_PFIXED                      = 0x1121200000000205,


        BATTERY_STORAGE                             = 0x1121210000060000,
        BATTERY_STORAGE_MAX_POWER                   = 0x1121210000060105,
        BATTERY_STORAGE_MIN_CAPACITY                = 0x1121210000060205,
        
        MEASUREMENT                                 = 0x1200000000000000,
        MEASUREMENT_DIRECTION                       = 0x120000000000010a,
        MEASUREMENT_TYPE                            = 0x120000000000020a,
        MEASUREMENT_SCADA_ADDRESS                   = 0x1200000000000307,
        MEASUREMENT_POWER_SYS_RESOURCE              = 0x1200000000000409,

        DISCRETE                                    = 0x1210000000010000,
        DISCRETE_MAX_VALUE                          = 0x1210000000010103,
        DISCRETE_MIN_VALUE                          = 0x1210000000010203,
        DISCRETE_NORMAL_VALUE                       = 0x1210000000010303,

        ANALOG                                      = 0x1220000000020000,
        ANALOG_MAX_VALUE                            = 0x1220000000020105,
        ANALOG_MIN_VALUE                            = 0x1220000000020205,
        ANALOG_NORMAL_VALUE                         = 0x1220000000020305,
        
        GEOGRAFICAL_REGION                          = 0x1300000000050000,
    }

    [Flags]
	public enum ModelCodeMask : long
	{
		MASK_TYPE			 = 0x00000000ffff0000,
		MASK_ATTRIBUTE_INDEX = 0x000000000000ff00,
		MASK_ATTRIBUTE_TYPE	 = 0x00000000000000ff,

		MASK_INHERITANCE_ONLY = unchecked((long)0xffffffff00000000),
		MASK_FIRSTNBL		  = unchecked((long)0xf000000000000000),
		MASK_DELFROMNBL8	  = unchecked((long)0xfffffff000000000),		
	}																		
}


