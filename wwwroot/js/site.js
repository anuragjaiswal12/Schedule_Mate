function patternValidator(feildtype, value) {
    // Validate first name and last name fields (letters only, minimum length of 2)
    const nameRegex = /^[A-Za-z\s]{2,}$/

    // Validate password field (minimum 8 characters, at least one uppercase letter, one lowercase letter, one special character, and one number)
    const passwordRegex = /^(?=.*\d)(?=.*[!@#$%^&*])(?=.*[a-z])(?=.*[A-Z]).{8,}$/

    //Validate email feild
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/ // /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/

    if (feildtype === 'email') {
        if (!emailRegex.test(value)) return false
    } else if (feildtype === 'name') {
        if (!nameRegex.test(value)) return false
    } else if (feildtype === 'pwd') {
        if (!passwordRegex.test(value)) return false
    }
    return true
}