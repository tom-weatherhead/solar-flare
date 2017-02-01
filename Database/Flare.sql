-- MySQL dump 10.13  Distrib 5.1.35, for Win32 (ia32)
--
-- Host: localhost    Database: flare
-- ------------------------------------------------------
-- Server version	5.1.35-community

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `accounts`
--

DROP TABLE IF EXISTS `accounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `accounts` (
  `AccountID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Enabled` char(1) NOT NULL,
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY (`AccountID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `contacts`
--

DROP TABLE IF EXISTS `contacts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `contacts` (
  `ContactID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `AccountID` int(10) unsigned NOT NULL,
  `Enabled` char(1) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `EmailAddress` varchar(255) NOT NULL,
  PRIMARY KEY (`ContactID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `systemconfiguration`
--

DROP TABLE IF EXISTS `systemconfiguration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `systemconfiguration` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Value` varchar(255) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `targetformfields`
--

DROP TABLE IF EXISTS `targetformfields`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `targetformfields` (
  `TargetID` int(10) unsigned NOT NULL,
  `FieldName` varchar(100) NOT NULL,
  `FieldValue` varchar(255) NOT NULL,
  `FieldID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`FieldID`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `targetlog`
--

DROP TABLE IF EXISTS `targetlog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `targetlog` (
  `LogID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `TargetID` int(10) unsigned NOT NULL,
  `TimeStamp` datetime NOT NULL,
  `Status` char(4) NOT NULL,
  `Message` text NOT NULL,
  `ErrorCode` int(10) unsigned NOT NULL,
  `LocationID` int(10) unsigned NOT NULL,
  `ResponseTime` int(10) unsigned NOT NULL,
  PRIMARY KEY (`LogID`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `targets`
--

DROP TABLE IF EXISTS `targets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `targets` (
  `TargetID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `AccountID` int(10) unsigned NOT NULL,
  `Enabled` char(1) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `URL` varchar(255) NOT NULL,
  `MonitorInterval` int(10) unsigned NOT NULL,
  `LastMonitoredAt` datetime DEFAULT NULL,
  `MonitorType` int(10) unsigned NOT NULL,
  `LastTargetLogID` int(10) unsigned NOT NULL,
  `TargetAddedAt` datetime NOT NULL,
  `LastFailedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`TargetID`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2009-09-30 17:01:03
