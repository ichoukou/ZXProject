CREATE TABLE `taskquerylog` (
  `Id` bigint(20) NOT NULL auto_increment,
  `CreateTime` datetime NOT NULL,
  `Status` int(11) NOT NULL,
  `UserAccount` varchar(100) NOT NULL,
  PRIMARY KEY  (`Id`)
)