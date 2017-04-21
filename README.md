
continue https://confluence.kingland.com/display/DHTD/Request+to+Enrollment+Turnaround+Reduction
DEV run courier
https://confluence.kingland.com/display/TRAC/CourierRunBook

WebEx meeting   
https://kingland.webex.com/join/bekle   |  921 327 441     


http://10.6.48.40:8080/otpexternal/login.html

SPA    MD5加密算法 密码

if(db.hasRows)
{
	Session["UserID"]=db.GetValue(0);
	Session["Role"]=db.GetValue(4);
}

--legal Structure
select distinct a.GMFID, b.Country, Actual_LegalStructure
from #LegalStructure a
left outer join #Countries b on a.gmfid = b.gmfid
where Actual_LegalStructure is not NULL

--Registered
select distinct GMFID, Country
from #Countries
where GMFID not in (
	select distinct GMFID
	from #LegalStructure
	where Actual_LegalStructure is not NULL)

select caOID, caGMFID, caFID, caType, caAddr1, caAddr2, caCity, caStPrAbrv, caStPrName, caCounty, caCountry, caPostal, caCityCode, caStPrCode, caCntyCode, caCtryCode, caContCode, caSrcCtryCode,  
CAST(NULL as nvarchar(3)) as chg_status, CAST(NULL as datetime) as chg_time  
into #tempForChange from DEV_GMF_PRD..tComAddr where caOID = -1;
insert into #tempForChange(caOID, caGMFID, caFID, caType, caAddr1, caAddr2, caCity, caStPrAbrv, caStPrName, caCounty, caCountry, caPostal, caCityCode, caStPrCode, caCntyCode, caCtryCode, caContCode, caSrcCtryCode, chg_status, chg_time)  
          select caOID, caGMFID, caFID, caType, caAddr1, caAddr2, caCity, caStPrAbrv, caStPrName, caCounty, caCountry, caPostal, caCityCode, caStPrCode, caCntyCode, caCtryCode, caContCode, caSrcCtryCode,  
          'bef', current_timestamp from DEV_GMF_PRD..tComAddr where
EXEC [DEV_GMF_PRD].[dbo].[GenerateChangeResasons] 
     @dataTabName='tComAddr', 
     @chgReason = 'StandardCh',
     @chgSource = 'StandardCh',
     @chgUser = 'cking'

set IDENTITY_INSERT #tempForChange ON

USE [DEV_GMF_PRD]
GO

/****** Object:  StoredProcedure [dbo].[GenerateChangeResasons]    Script Date: 06/04/2017 11:00:48 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GenerateChangeResasons]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GenerateChangeResasons]
GO

USE [DEV_GMF_PRD]
GO

/****** Object:  StoredProcedure [dbo].[GenerateChangeResasons]    Script Date: 06/04/2017 11:00:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GenerateChangeResasons]
(	
	--==========
	--parameter: 
	--==========
	@dataTabName nvarchar(100),
	--@tempChgTabName nvarchar(100) = '#tempForChange',
	@chgReason nvarchar(20) = null,
	@chgSource nvarchar(20) = null,
	@chgUser nvarchar(20) = null
	--@chgTime datetime = null
)
AS
BEGIN

	--- for Com Name
	if (@dataTabName='tComName')
	begin
	declare @nameKeyBf bigint, @cGmfid bigint, @cFid int, @impTypeBf nvarchar(10), @typeBf nvarchar(10), @typeIncBf int, @nameStrBf nvarchar(255),
		@nameKeyAf bigint, @impTypeAf nvarchar(10), @typeAf nvarchar(10), @typeIncAf int, @nameStrAf nvarchar(255), @chgTime datetime, @EventID BIGINT
		
	--- Loop after records
	declare dataChgCur cursor for
		select cnOID, cnGMFID, cnFID, cnImpType, cnType, cnTypeInc, cnName, chg_time
		from #tempForChange where chg_status='aft'

	open dataChgCur
	fetch next from dataChgCur into @nameKeyAf, @cGmfid, @cFid, @impTypeAf, @typeAf, @typeIncAf, @nameStrAf, @chgTime
	while @@Fetch_Status = 0 
	begin

		set @nameKeyBf = null
		---  if there is before record, insert UPDATE change record
		select @nameKeyBf = cnOID, @impTypeBf=cnImpType, @typeBf=cnType, @typeIncBf=cnTypeInc, @nameStrBf=cnName from #tempForChange where cnOID =@nameKeyAf and chg_status='bef'
		if (@nameKeyBf is not null)
		begin
			INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@cGmfid, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;

			Insert into tChgCName (hcnOid, hcnGMFID, hcnFID, hcnEventID, hcnAction, hcnImpTypeBef, hcnImpTypeAft, hcnTypeBef, hcnTypeAft, hcnTypeIncBef, hcnTypeIncAft, hcnNameBef, hcnNameAft)
			values (@nameKeyAf, @cGmfid, @cFid, @EventID, 'U', @impTypeBf, @impTypeAf, @typeBf, @typeAf, @typeIncBf, @typeIncAf, @nameStrBf, @nameStrAf)
		end
		--- if there is no before record, insert ADD change record
		else
		begin
			INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@cGmfid, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;
			
			Insert into tChgCName (hcnOid, hcnGMFID, hcnFID, hcnEventID, hcnAction, hcnImpTypeAft, hcnTypeAft, hcnTypeIncAft, hcnNameAft)
			values (@nameKeyAf, @cGmfid, @cFid, @EventID, 'I', @impTypeAf, @typeAf, @typeIncAf, @nameStrAf)
		end

		fetch next from dataChgCur into @nameKeyAf, @cGmfid, @cFid, @impTypeAf, @typeAf, @typeIncAf, @nameStrAf, @chgTime
	end
	close dataChgCur
	deallocate dataChgCur
	--- end of loop After Records
	
	--- Loop Before without After (delete case)
	declare delDataChgCur cursor for select bf.cnOID, bf.cnGMFID, bf.cnFID, bf.chg_time from 
		(select cnOID, cnGMFID, cnFID, chg_time from #tempForChange where chg_status='bef') bf left join
		(select cnOID from #tempForChange where chg_status='aft') af on bf.cnOID = af.cnOID where af.cnOID is null
		
	open delDataChgCur
	fetch next from delDataChgCur into @nameKeyBf, @cGmfid, @cFid, @chgTime
	while @@Fetch_Status = 0 
	begin
		INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@cGmfid, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;
		Insert into tChgCName (hcnOid, hcnGMFID, hcnFID, hcnEventID, hcnAction, hcnImpTypeBef, hcnTypeBef, hcnTypeIncBef, hcnNameBef)
			select (cnOid, cnGMFID, cnFID, @EventID, 'D', cnImpType, cnType, cnTypeInc, cnName) from #tempForChange where cnOid = @nameKeyBf and chg_status='bef'
	
		fetch next from delDataChgCur into @nameKeyBf, @cGmfid, @cFid, @chgTime
	end
	close delDataChgCur
	deallocate delDataChgCur
	--- end of Loop Before without After (delete case)
	end
	-----------===============================================================================
	--- for Com Address
	else if (@dataTabName='tComAddr')
	begin	
	declare @addKeyBf bigint, @cGmfid bigint, @cFid int, @typeBf nvarchar(10), @add1Bf nvarchar(255), @add2Bf nvarchar(255), @cityBf nvarchar(255), @stPrAbrvBf nvarchar(20), @stPrNameBf nvarchar(255),
		@countyBf nvarchar(255), @countryBf nvarchar(255), @postalBf nvarchar(20), @cityCodeBf nvarchar(20), @stPrCodeBf nvarchar(20), @cntyCodeBf nvarchar(20), @ctryCodeBf nvarchar(3), @contCodeBf nvarchar(3), @srcCtryCodeBf nvarchar(20),
		@addKeyAf bigint, @typeAf nvarchar(10), @add1Af nvarchar(255), @add2Af nvarchar(255), @cityAf nvarchar(255), @stPrAbrvAf nvarchar(20), @stPrNameAf nvarchar(255),
		@countyAf nvarchar(255), @countryAf nvarchar(255), @postalAf nvarchar(20), @cityCodeAf nvarchar(20), @stPrCodeAf nvarchar(20), @cntyCodeAf nvarchar(20), @ctryCodeAf nvarchar(3), @contCodeAf nvarchar(3), @srcCtryCodeAf nvarchar(20),
		@chgTime datetime, @EventID BIGINT
		
	--- Loop of After Records
	declare dataChgCur cursor for
		select caOID, caGMFID, caFID, caType, caAddr1, caAddr2, caCity, caStPrAbrv, caStPrName, caCounty, caCountry, caPostal, caCityCode, caStPrCode, caCntyCode, caCtryCode, caContCode, caSrcCtryCode, chg_time
		from #tempForChange where chg_status='aft'

	open dataChgCur
	fetch next from dataChgCur into @addKeyAf, @cGmfid, @cFid, @typeAf, @add1Af, @add2Af, @cityAf, @stPrAbrvAf, @stPrNameAf, @countyAf, @countryAf, @postalAf, @cityCodeAf, @stPrCodeAf, @cntyCodeAf, @ctryCodeAf, @contCodeAf, @srcCtryCodeAf, @chgTime
	while @@Fetch_Status = 0 
	begin

		set @addKeyBf = null
		---  if there is before record, insert UPDATE change record
		select @addKeyBf = caOID, @typeBf = caType, @add1Bf = caAddr1, @add2Bf = caAddr2, @cityBf = caCity, @stPrAbrvBf = caStPrAbrv, @stPrNameBf = caStPrName, 
			@countyBf = caCounty, @countryBf = caCountry, @postalBf = caPostal, @cityCodeBf = caCityCode, @stPrCodeBf = caStPrCode, @cntyCodeBf = caCntyCode, 
			@ctryCodeBf = caCtryCode, @contCodeBf = caContCode, @srcCtryCodeBf = caSrcCtryCode from #tempForChange where caOID =@addKeyAf and chg_status='bef'
		if (@addKeyBf is not null)
		begin
			INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@cGmfid, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;

			Insert into tChgCAddr (hcaOid, hcaGMFID, hcaFID, hcaEventID, hcaAction, hcaTypeBef, hcaTypeAft, hcaAddr1Bef, hcaAddr1Aft, hcaAddr2Bef, hcaAddr2Aft, hcaCityBef, hcaCityAft, hcaStPrAbrvBef, hcaStPrAbrvAft, hcaStPrNameBef, hcaStPrNameAft, hcaCountyBef, hcaCountyAft, hcaCountryBef, hcaCountryAft, hcaPostalBef, hcaPostalAft, hcaCityCodeBef, hcaCityCodeAft, hcaStPrCodeBef, hcaStPrCodeAft, hcaCntyCodeBef, hcaCntyCodeAft, hcaCtryCodeBef, hcaCtryCodeAft, hcaContCodeBef, hcaContCodeAft, hcaSrcCtryCodeBef, hcaSrcCtryCodeAft)
			values (@addKeyAf, @cGmfid, @cFid, @EventID, 'U', @typeBf, @typeAf, @add1Bf, @add1Af, @add2Bf, @add2Af, @cityBf, @cityAf, @stPrAbrvBf, @stPrAbrvAf, @stPrNameBf, @stPrNameAf, @countyBf, @countyAf, @countryBf, @countryAf, @postalBf, @postalAf, @cityCodeBf, @cityCodeAf, @stPrCodeBf, @stPrCodeAf, @cntyCodeBf, @cntyCodeAf, @ctryCodeBf, @ctryCodeAf, @contCodeBf, @contCodeAf, @srcCtryCodeBf, @srcCtryCodeAf)
		end
		else
		begin
		--- if there is no before record, insert ADD change record
			INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@cGmfid, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;
			Insert into tChgCAddr (hcaOid, hcaGMFID, hcaFID, hcaEventID, hcaAction, hcaTypeAft, hcaAddr1Aft, hcaAddr2Aft, hcaCityAft, hcaStPrAbrvAft, hcaStPrNameAft, hcaCountyAft, hcaCountryAft, hcaPostalAft, hcaCityCodeAft, hcaStPrCodeAft, hcaCntyCodeAft, hcaCtryCodeAft, hcaContCodeAft, hcaSrcCtryCodeAft)
			values (@addKeyAf, @cGmfid, @cFid, @EventID, 'I', @typeBf, @typeAf, @add1Af, @add2Af, @cityAf, @stPrAbrvAf, @stPrNameAf, @countyAf, @countryAf, @postalAf, @cityCodeAf, @stPrCodeAf, @cntyCodeAf, @ctryCodeAf, @contCodeAf, @srcCtryCodeAf)
		end

		fetch next from dataChgCur into @addKeyAf, @cGmfid, @cFid, @typeAf, @add1Af, @add2Af, @cityAf, @stPrAbrvAf, @stPrNameAf, @countyAf, @countryAf, @postalAf, @cityCodeAf, @stPrCodeAf, @cntyCodeAf, @ctryCodeAf, @contCodeAf, @srcCtryCodeAf, @chgTime
	end
	close dataChgCur
	deallocate dataChgCur
	--- end of Loop of After Records
	
	--- Loop Before without After (delete case)
	declare delDataChgCur cursor for select bf.caOID, bf.caGMFID, bf.caFID, bf.chg_time from 
		(select caOID, caGMFID, caFID, chg_time from #tempForChange where chg_status='bef') bf left join
		(select caOID from #tempForChange where chg_status='aft') af on bf.caOID = af.caOID where af.caOID is null
		
	open delDataChgCur
	fetch next from delDataChgCur into @addKeyBf, @cGmfid, @cFid, @chgTime
	while @@Fetch_Status = 0 
	begin
		INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@cGmfid, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;
		Insert into tChgCAddr (hcaOid, hcaGMFID, hcaFID, hcaEventID, hcaAction, hcaTypeBef, hcaAddr1Bef, hcaAddr2Bef, hcaCityBef, hcaStPrAbrvBef, hcaStPrNameBef, hcaCountyBef, hcaCountryBef, hcaPostalBef, hcaCityCodeBef, hcaStPrCodeBef, hcaCntyCodeBef, hcaCtryCodeBef, hcaContCodeBef, hcaSrcCtryCodeBef)
			select (caOID, caGMFID, caFID, @EventID, 'D', , caType, caAddr1, caAddr2, caCity, caStPrAbrv, caStPrName, caCounty, caCountry, caPostal, caCityCode, caStPrCode, caCntyCode, caCtryCode, caContCode, caSrcCtryCode) from #tempForChange where caOid = @addKeyBf and chg_status='bef'
	
		fetch next from delDataChgCur into @addKeyBf, @cGmfid, @cFid, @chgTime
	end
	close delDataChgCur
	deallocate delDataChgCur
	--- end of Loop Before without After (delete case)
	end
	-----------===============================================================================
	--- for Rel Data
	else if (@dataTabName='tRelData')
	begin

	declare @relKeyBf bigint, @relKeyAf bigint, @cFid int, @vrdInactiveBef tinyint, @vrdInactiveAft tinyint, @vrdInactRsnBef nvarchar(10), @vrdInactRsnAft nvarchar(10), @vrdInactDtBef datetime, @vrdInactDtAft datetime, 
		@vrdExpiredBef tinyint, @vrdExpiredAft tinyint, @vrdExpireDtBef datetime, @vrdExpireDtAft datetime, @vrdExpRsnBef nvarchar(10), @vrdExpRsnAft nvarchar(10), 
		@vrdChdGMFIDBef bigint, @vrdChdGMFIDAft bigint, @vrdParGMFIDBef bigint, @vrdParGMFIDAft bigint, @vrdSubTypeBef nvarchar(10), @vrdSubTypeAft nvarchar(10),
		@vrdIsControllingBef tinyint, @vrdIsControllingAft tinyint, @vrdDUPBef bigint, @vrdDUPAft bigint, @vrdGUPBef bigint, @vrdGUPAft bigint, 
		@vrdPercentBef decimal(9, 8), @vrdPercentAft decimal(9, 8)
		
	--- Loop of After Records
	declare dataChgCur cursor for
		select rdGMFID, rdFID, rdInactive, rdInactRsn, rdInactDt, rdExpired, rdExpireDt, rdExpRsn, rdChdGMFID, rdParGMFID, rdSubType, rdIsControlling, rdDUP, rdGUP, rdPercent, chg_time
		from #tempForChange where chg_status='aft'

	open dataChgCur
	fetch next from dataChgCur into @relKeyAf, @cFid, @vrdInactiveAft, @vrdInactRsnAft, @vrdInactDtAft, @vrdExpiredAft, @vrdExpireDtAft, @vrdExpRsnAft, @vrdChdGMFIDAft, @vrdParGMFIDAft, @vrdSubTypeAft, @vrdIsControllingAft, @vrdDUPAft, @vrdGUPAft, @vrdPercentAft, @chgTime
	while @@Fetch_Status = 0 
	begin
		set @relKeyBf = null
		select @relKeyBf = rdGMFID, @vrdInactiveBef = rdInactive, @vrdInactRsnBef = rdInactRsn, @vrdInactDtBef = rdInactDt, @vrdExpiredBef = rdExpired, @vrdExpireDtBef = rdExpireDt, @vrdExpRsnBef = rdExpRsn, 
			@vrdChdGMFIDBef = rdChdGMFID, @vrdParGMFIDBef = rdParGMFID, @vrdSubTypeBef = rdSubType, 
			@vrdIsControllingBef = rdIsControlling, @vrdDUPBef = rdDUP, @vrdGUPBef = rdGUP, @vrdPercentBef = rdPercent from #tempForChange where rdGMFID =@relKeyAf and rdFID = @cFid and chg_status='bef'
		---  if there is before record, insert UPDATE change record
		if (@relKeyBf is not null)
		begin
			INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@relKeyAf, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;

			Insert into tChgCAddr (hrdGMFID, hrdFID, hrdEventID, hrdAction, hrdInactiveBef, hrdInactiveAft, hrdInactRsnBef, hrdInactRsnAft, hrdInactDtBef, hrdInactDtAft, hrdExpiredBef, hrdExpiredAft, hrdExpireDtBef, hrdExpireDtAft, hrdExpRsnBef, hrdExpRsnAft, hrdChdGMFIDBef, hrdChdGMFIDAft, hrdParGMFIDBef, hrdParGMFIDAft, hrdSubTypeBef, hrdSubTypeAft, hrdIsControllingBef, hrdIsControllingAft, hrdDUPBef, hrdDUPAft, hrdGUPBef, hrdGUPAft, hrdPercentBef, hrdPercentAft)
			values (@relKeyAf, @cFid, @EventID, 'U', @vrdInactiveBef, @vrdInactiveAft, @vrdInactRsnBef, @vrdInactRsnAft, @vrdInactDtBef, @vrdInactDtAft, @vrdExpiredBef, @vrdExpiredAft, @vrdExpireDtBef, @vrdExpireDtAft, @vrdExpRsnBef, @vrdExpRsnAft, @vrdChdGMFIDBef, @vrdChdGMFIDAft, @vrdParGMFIDBef, @vrdParGMFIDAft, @vrdSubTypeBef, @vrdSubTypeAft, @vrdIsControllingBef, @vrdIsControllingAft, @vrdDUPBef, @vrdDUPAft, @vrdGUPBef, @vrdGUPAft, @vrdPercentBef, @vrdPercentAft)
		end
		--- if there is no before record, insert ADD change record
		else
		begin
			INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@relKeyAf, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;
			Insert into tChgCAddr (hrdGMFID, hrdFID, hrdEventID, hrdAction, hrdInactiveAft, hrdInactRsnAft, hrdInactDtAft, hrdExpiredAft, hrdExpireDtAft, hrdExpRsnAft, hrdChdGMFIDAft, hrdParGMFIDAft, hrdSubTypeAft, hrdIsControllingAft, hrdDUPAft, hrdGUPAft, hrdPercentAft)
			values (@relKeyAf, @cFid, @EventID, 'I', @vrdInactiveAft, @vrdInactRsnAft, @vrdInactDtAft, @vrdExpiredAft, @vrdExpireDtAft, @vrdExpRsnAft, @vrdChdGMFIDAft, @vrdParGMFIDAft, @vrdSubTypeAft, @vrdIsControllingAft, @vrdDUPAft, @vrdGUPAft, @vrdPercentAft)
		end

		fetch next from dataChgCur into @relKeyAf, @cFid, @vrdInactiveAft, @vrdInactRsnAft, @vrdInactDtAft, @vrdExpiredAft, @vrdExpireDtAft, @vrdExpRsnAft, @vrdChdGMFIDAft, @vrdParGMFIDAft, @vrdSubTypeAft, @vrdIsControllingAft, @vrdDUPAft, @vrdGUPAft, @vrdPercentAft, @chgTime
	end
	close dataChgCur
	deallocate dataChgCur
	--- end of Loop of After Records
	
	--- Loop Before without After (delete case)
	declare delDataChgCur cursor for select bf.rdGMFID, bf.rdFID, bf.chg_time from 
		(select rdGMFID, rdFID, chg_time from #tempForChange where chg_status='bef') bf left join
		(select rdGMFID, rdFID from #tempForChange where chg_status='aft') af on bf.rdGMFID = af.rdGMFID and bf.rdFID = af.rdFID where af.rdFID is null
		
	open delDataChgCur
	fetch next from delDataChgCur into @relKeyAf, @cFid, @chgTime
	while @@Fetch_Status = 0 
	begin
		INSERT INTO tChgEvent (heGMFID, heFID, heReason, heSource, heUserID, heChgDate, heActive)
			VALUES (@relKeyAf, @cFid, @chgReason, @chgSource, @chgUser, @chgTime, 1)
			SET @EventID = @@IDENTITY;
		Insert into tChgCAddr (hrdGMFID, hrdFID, hrdEventID, hrdAction, hrdInactiveBef, hrdInactRsnBef, hrdInactDtBef, hrdExpiredBef, hrdExpireDtBef, hrdExpRsnBef, hrdChdGMFIDBef, hrdParGMFIDBef, hrdSubTypeBef, hrdIsControllingBef, hrdDUPBef, hrdGUPBef, hrdPercentBef)
			select (rdGMFID, rdFID, @EventID, 'D', rdInactive, rdInactRsn, rdInactDt, rdExpired, rdExpireDt, rdExpRsn, rdChdGMFID, rdParGMFID, rdSubType, rdIsControlling, rdDUP, rdGUP, rdPercent) from #tempForChange where rdGMFID = @relKeyAf and rdFID = @cFid and chg_status='bef'
	
		fetch next from delDataChgCur into @relKeyAf, @cGmfid, @cFid, @chgTime
	end
	close delDataChgCur
	deallocate delDataChgCur
	--- end of Loop Before without After (delete case)

	end

END
GO
