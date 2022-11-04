import axios from "axios";

/*eslint-disable */
var ProfileService = {
    getOrCreatePersonalSpace: async function(profile){
        var personalSpaceKey = profile.email.toUpperCase();
        var getPersonalSpaceRequest = await axios.get(
            "/api/space/" + personalSpaceKey,
            {
                validateStatus: false,
            }
        );
        if (getPersonalSpaceRequest.status == 200) return personalSpaceKey;
        var createPersonalSpaceRequest = await axios.post(
            "/api/space",
            {
                key: personalSpaceKey,
                name: profile.name,
                type: "Personal",
                description: "my personal space",
            },
            {
                validateStatus: false,
            }
        );
        if (createPersonalSpaceRequest.status != 200) {
            alert(
                createPersonalSpaceRequest.statusText +
                "\n" +
                createPersonalSpaceRequest.data
            );
            // eslint-disable-next-line
            throw new Exception();
        }
        return personalSpaceKey;
    }
}
export default ProfileService
/* eslint-enable */