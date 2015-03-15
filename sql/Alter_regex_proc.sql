CREATE DEFINER=`bryan`@`%` PROCEDURE `alter_regex`(IN oldDisplayName VARCHAR(500), IN regexVal BLOB, IN newDisplayNameVal VARCHAR(500), IN indicesVal BLOB, IN editorEmail VARCHAR(500), OUT success BIT)
begin
	DECLARE uniq INT DEFAULT 0;
    SET success = 0;
	SELECT COUNT(*) INTO uniq FROM type_regex WHERE DisplayName = newDisplayNameVal;
    IF (uniq = 0 OR newDisplayNameVal = NULL) AND regexVal != NULL AND oldDisplayNameVal != NULL AND indicesVal != NULL THEN
    BEGIN
		IF newDisplayNameVal = NULL THEN
			UPDATE type_regex SET Regex = regexVal,  Indices = indicesVal, DateModified = NOW(), LastModifiedBy = editorEmail WHERE DisplayName = oldDisplayName;
		else
			UPDATE type_regex SET Regex = regexVal,  DisplayName = newDisplayNameVal, Indices = indicesVal, DateModified = NOW(), LastModifiedBy = editorEmail WHERE DisplayName = oldDisplayName;
        
        END IF;
        SET success = 1;
    END;
    END IF;
END