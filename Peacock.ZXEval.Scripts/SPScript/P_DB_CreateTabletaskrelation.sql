
CREATE TABLE `taskrelation` (
  `Id` bigint(20) NOT NULL auto_increment,
  `CreateTIme` datetime default NULL,
  `UserAccount` varchar(100) default NULL,
  `RelationId` bigint(20) default NULL,
  `RelationType` int(11) default NULL,
  `Tasknum` varchar(100) default NULL,
  PRIMARY KEY  (`Id`)
)