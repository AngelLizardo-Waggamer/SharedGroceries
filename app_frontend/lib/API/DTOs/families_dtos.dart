import 'dto.dart';

class CreateFamilyDTO extends DTO {
	final String familyName;

	CreateFamilyDTO({
		required this.familyName
	});

	@override
	Map<String, dynamic> toJson() {
		return {
			'familyName': familyName
		};
	}
}

class JoinFamilyDTO extends DTO {
	final String inviteCode;

	JoinFamilyDTO({
		required this.inviteCode
	});

	@override
	Map<String, dynamic> toJson() {
		return {
			'inviteCode': inviteCode
		};
	}
}

class LeaveFamilyDTO extends DTO {
	@override
	Map<String, dynamic> toJson() {
		return {};
	}
}