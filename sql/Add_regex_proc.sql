CREATE DEFINER=`bryan`@`%` PROCEDURE `add_regex`(IN regexVal BLOB, IN displayNameVal VARCHAR(500), IN indicesVal BLOB, IN creatorEmail VARCHAR(500), OUT success BIT)
begin
	DECLARE uniq INT DEFAULT 0;
    SET success = 0;
	SELECT COUNT(*) INTO uniq FROM type_regex WHERE DisplayName = displayNameVal;
    IF uniq = 0 AND regexVal != NULL AND displayNameVal != NULL AND indicesVal != NULL THEN
    BEGIN
		INSERT INTO type_regex ( Regex, DisplayName, Indices, DateCreated, CreatedBy, DateModified, LastModifiedBy) VALUES (
			regexVal, displayNameVal, indicesVal, NOW(), creatorEmail, NOW(), creatorEmail);
		SET success = 1;
    END;
    END IF;
END