$PBExportHeader$test-ftp4pb.sra
$PBExportComments$Generated Application Object
forward
global type test-ftp4pb from application
end type
global transaction sqlca
global dynamicdescriptionarea sqlda
global dynamicstagingarea sqlsa
global error error
global message message
end forward

global type test-ftp4pb from application
string appname = "test-ftp4pb"
string appruntimeversion = "19.2.0.2728"
end type
global test-ftp4pb test-ftp4pb

on test-ftp4pb.create
appname = "test-ftp4pb"
message = create message
sqlca = create transaction
sqlda = create dynamicdescriptionarea
sqlsa = create dynamicstagingarea
error = create error
end on

on test-ftp4pb.destroy
destroy( sqlca )
destroy( sqlda )
destroy( sqlsa )
destroy( error )
destroy( message )
end on

