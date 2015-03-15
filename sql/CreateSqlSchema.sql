-- MySQL dump 10.13  Distrib 5.6.17, for Win32 (x86)
--
-- Host:     Database: ieee
-- ------------------------------------------------------
-- Server version	5.6.21-log

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
-- Table structure for table `type_regex`
--

DROP TABLE IF EXISTS `type_regex`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `type_regex` (
  `RegexId` int(11) NOT NULL AUTO_INCREMENT,
  `Regex` blob NOT NULL COMMENT 'The regex itself.',
  `DisplayName` varchar(500) NOT NULL COMMENT 'Name displayed to the user, must be unique.',
  `Indices` blob NOT NULL COMMENT 'A json object of the mapping between indicies of the matching groups and the data.',
  `DateCreated` datetime DEFAULT NULL,
  `CreatedBy` varchar(500) DEFAULT NULL COMMENT 'Email of person who created the regex.',
  `DateModified` datetime DEFAULT NULL,
  `LastModifiedBy` varchar(500) DEFAULT NULL COMMENT 'Email of person who last modified the regex.',
  PRIMARY KEY (`RegexId`),
  UNIQUE KEY `RegexId_UNIQUE` (`RegexId`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8 COMMENT='Table of regular expressions for parsing card inputs from various cards and institutions.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `type_regex`
--

LOCK TABLES `type_regex` WRITE;
/*!40000 ALTER TABLE `type_regex` DISABLE KEYS */;
INSERT INTO `type_regex` VALUES (1,'^%(\\w+)\\^(\\d+)\\^{3}(\\d+)\\^(\\w+),\\s(?:([\\w\\s]+)\\s(\\w{1})\\?;|([\\w\\s]+)\\?;)(\\d+)=(\\d+)\\?$','University of Minnesota','{\"firstName\":\"5,7\",\"lastName\":\"4\",\"middleName\":\"6\",\"studentId\":\"2\",\"email\":\"-1\"}','2015-01-27 16:28:00','stadi012@umn.edu','2015-01-27 16:28:00','stadi012@umn.edu'),(2,'^%(\\w{2})(\\w*)\\^(\\w+)\\s([\\w\\s]+)\\s(\\w+)\\^([\\w\\s]+)\\^\\?;(\\d+)=(\\d+)\\?#\\\"\\s(\\d+)\\s+(\\w)\\s+(\\w)(\\d{3})(\\d{3})\\s+(\\w+)\\s+\\[\\\\%\\\"\\](\\w)\\\'\\s+\\?','Minnesota Driver\'s License','{\"firstName\":\"3\",\"lastName\":\"5\",\"middleName\":\"4\",\"studentId\":\"-1\",\"email\":\"-1\"}','2015-02-06 21:05:00','stadi012@umn.edu','2015-02-06 21:05:00','stadi012@umn.edu');
/*!40000 ALTER TABLE `type_regex` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `type_themes`
--

DROP TABLE IF EXISTS `type_themes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `type_themes` (
  `ThemeId` int(11) NOT NULL AUTO_INCREMENT,
  `Theme` blob NOT NULL COMMENT 'JSON object of the theme.',
  `DisplayName` varchar(500) NOT NULL COMMENT 'Unique name of the theme to be displayed to the user.',
  `DateCreated` datetime DEFAULT NULL,
  `DateModified` datetime DEFAULT NULL,
  PRIMARY KEY (`ThemeId`),
  UNIQUE KEY `ThemeId_UNIQUE` (`ThemeId`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8 COMMENT='Predefined themes availale loaded for use.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `type_themes`
--

LOCK TABLES `type_themes` WRITE;
/*!40000 ALTER TABLE `type_themes` DISABLE KEYS */;
INSERT INTO `type_themes` VALUES (1,'{\"bodyBackgroundColor\":\"006699\", \"buttonBackgroundColor\":\"39b3d7\", \"bodyColor\":\"ffffff\", \"themeShade\":\"light\", \"imageUrl\":\"../Images/logo.svg\", \"headerText\":\"University of Minnesota\", \"useSwipe\":\"true\"}','UMN IEEE','2015-01-27 22:24:00','2015-01-27 22:24:00'),(2,'{\"bodyBackgroundColor\":\"bb2cd1\", \"buttonBackgroundColor\":\"d63a54\", \"bodyColor\":\"00ff1e\", \"themeShade\":\"light\", \"imageUrl\":\"http://goo.gl/bzHpYs\", \"headerText\":\"Cat Lovers of America\", \"useSwipe\":\"true\"} ','Cat Lovers of America','2015-01-27 22:24:00','2015-01-27 22:24:00'),(3,'{\"bodyBackgroundColor\":\"a058c9\",\"buttonBackgroundColor\":\"5f0594\",\"bodyColor\":\"ffffff\",\"themeShade\":\"light\",\"imageUrl\":\"http://ieee.griet.ac.in/wp-content/uploads/2014/04/ieee_wie_purple.png\",\"headerText\":\"University of Minnesota\",\"useSwipe\":\"true\"}','UMN IEEE WiE','2015-02-07 16:50:00','2015-02-07 16:50:00'),(4,'{\"bodyBackgroundColor\":\"ffd018\",\"buttonBackgroundColor\":\"9e1313\",\"bodyColor\":\"000000\",\"themeShade\":\"dark\",\"imageUrl\":\"http://www.mngofirst.org/uploads/6/3/5/8/6358288/1401824902.png\",\"headerText\":\"GOFIRST\",\"useSwipe\":\"true\"}','GOFIRST','2015-03-03 19:44:00','2015-03-03 19:44:00');
/*!40000 ALTER TABLE `type_themes` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2015-03-15 17:57:39
