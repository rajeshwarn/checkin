# Fill these values out before executing
SET @regex = ""; # the regex to add
SET @displayName = ""; # the name of the regex as seen on the website (must be unique)
SET @indices = "{\"firstName\":\"-1\",\"lastName\":\"-1\",\"middleName\":\"-1\",\"studentId\":\"-1\",\"email\":\"-1\"}"; # the json object of mapping of indicies of match groups to data elements
# -1 if data not present
# list of comma seperated integers if variable match grouping indicies (evaluates first to last listed) (occurs with conditionals in regex)
# first name and last name must be present
# UMN example "{"firstName":"5,7","lastName":"4","middleName":"6","studentId":"2","email":"-1"}"
SET @creatorEmail = ""; # email of person who created regex

SET @success = 0; # success/failure boolean
CALL add_regex(@regex, @displayName, @indices, @creatorEmail, @success);
SELECT @success; #1 if success, 0 if fail