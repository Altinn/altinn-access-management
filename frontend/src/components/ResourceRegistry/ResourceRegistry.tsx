import { FC, useState } from 'react';
import classes from './ResourceRegistry.module.css';
import { TextField, Button } from '@altinn/altinn-design-system';
import { PopoverPanel } from '../Common/General/Panel';
import { ReactComponent as HelpIcon } from './HelpIcon.svg';
import Select from 'react-select';

const ResourceRegistry: FC = () => {
  const [isValidIdentifier, setIsValidIdentifier] = useState(true);
  const [isValidTitle, setIsValidTitle] = useState(true);
  const [isValidDescription, setIsValidDescription] = useState(true);
  const [isValidOrganizationNumber, setIsValidOrganizationNumber] =
    useState(true);
  const [isValidOrgCode, setIsValidOrgCode] = useState(true);
  const [isValidPhone, setIsValidPhone] = useState(true);
  const [isValidPartOf, setIsValidPartOf] = useState(true);
  const [isValidHomepage, setIsValidHomepage] = useState(true);
  const [isValidEmail, setIsValidEmail] = useState(true);
  const [selectTypes, setSelectTypes] = useState([]);
  const [selectSectors, setSelectSectors] = useState([]);
  const [id, setId] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [organizationNumber, setOrganizationNumber] = useState('');
  const [orgCode, setOrgcode] = useState('');
  const [phone, setPhone] = useState('');
  const [partOf, setPartOf] = useState('');
  const [homepage, setHomepage] = useState('');
  const [email, setEmail] = useState('');

  const types = [
    { value: 'type1', label: 'Type 1' },
    { value: 'type2', label: 'Type 2' },
    { value: 'type3', label: 'Type 3' },
    { value: 'type4', label: 'Type 4' },
    { value: 'type5', label: 'Type 5' },
    { value: 'type6', label: 'Type 6' },
    { value: 'type7', label: 'Type 7' },
    { value: 'type8', label: 'Type 8' },
  ];

  const sectors = [
    { value: 'sector1', label: 'Sector 1' },
    { value: 'sector2', label: 'Sector 2' },
    { value: 'sector3', label: 'Sector 3' },
    { value: 'sector4', label: 'Sector 4' },
    { value: 'sector5', label: 'Sector 5' },
    { value: 'sector6', label: 'Sector 6' },
    { value: 'sector7', label: 'Sector 7' },
    { value: 'sector8', label: 'Sector 8' },
  ];

  const resourceFormResult = {
    identifier: '',
    description: {
      nb: '',
    },
    title: {
      nb: '',
    },
    hasCompetentAuthority: {
      organization: '',
      orgCode: '',
    },
    contactPoint: {
      phone: '',
      email: '',
    },
    isPartOf: '',
    homepage: '',
    type: [],
    sector: [],
  };

  const validateEmailRegexp = (email: string) => {
    return String(email)
      .toLowerCase()
      .match(
        /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
      );
  };

  const validateOrgNr = (orgNr: string) => {
    return String(orgNr)
      .toLowerCase()
      .match(/^\d{9}$/);
  };

  const emailIsValid = (email: string) => {
    if (validateEmailRegexp(email)) {
      return true;
    } else {
      return false;
    }
  };

  const stringIsEmpty = (text: string) => {
    if (text.length == 0) {
      return true;
    } else {
      return false;
    }
  };

  const organizationNumberIsValid = (orgNr: string) => {
    if (validateOrgNr(orgNr)) {
      return true;
    } else {
      return false;
    }
  };

  const handleIdentifierChange = (event: string) => {
    setId(event);
    setIsValidIdentifier(!stringIsEmpty(id));
  };

  const handleTitleChange = (event: string) => {
    setTitle(event);
    setIsValidTitle(!stringIsEmpty(title));
  };

  const handleDescrptionChange = (event: string) => {
    setDescription(event);
    setIsValidDescription(!stringIsEmpty(description));
  };

  const handleOrganizationNumberChange = (event: string) => {
    setOrganizationNumber(event);
    setIsValidOrganizationNumber(organizationNumberIsValid(event));
  };

  const handleOrgCodeChange = (event: string) => {
    setOrgcode(event);
    setIsValidOrgCode(!stringIsEmpty(event));
  };

  const handlePhoneChange = (event: string) => {
    setPhone(event);
    setIsValidPhone(!stringIsEmpty(event));
  };

  const handleEmailChange = (event: string) => {
    setEmail(event);
    setIsValidEmail(emailIsValid(event));
  };

  const handleIsPartOfChange = (event: string) => {
    setPartOf(event);
    setIsValidPartOf(!stringIsEmpty(event));
  };

  const handleHomepageChange = (event: string) => {
    setHomepage(event);
    setIsValidHomepage(!stringIsEmpty(event));
  };

  const submit = () => {
    Object.values(selectSectors).forEach((sector: unknown) => {
      return resourceFormResult.sector.push(sector.value);
    });
    Object.values(selectTypes).forEach((type) => {
      resourceFormResult.type.push(type.value);
    });
    resourceFormResult.identifier = id;
    resourceFormResult.title.nb = title;
    resourceFormResult.description.nb = description;
    resourceFormResult.hasCompetentAuthority.organization = organizationNumber;
    resourceFormResult.hasCompetentAuthority.orgCode = orgCode;
    resourceFormResult.contactPoint.phone = phone;
    resourceFormResult.contactPoint.email = email;
    resourceFormResult.isPartOf = partOf;
    resourceFormResult.homepage = homepage;
    isAllFieldsValid();
    console.log(resourceFormResult);
  };

  const isAllFieldsValid = () => {
    if (
      !(
        isValidIdentifier &&
        isValidTitle &&
        isValidDescription &&
        isValidOrganizationNumber &&
        isValidOrgCode &&
        isValidPhone &&
        isValidPartOf &&
        isValidHomepage &&
        isValidEmail
      )
    ) {
      alert('Se over at alle felter er riktig utfylt');
    }
  };

  const questionMark = (
    <span className={classes['question-icon']}>
      <HelpIcon width={22} height={22}></HelpIcon>
    </span>
  );

  return (
    <div className={classes['resource-registry']}>
      <div className={classes['resource-registry__form']}>
        <div className={classes['button-container']}>
          <h2 className={classes['center']}>Ressursregistrering</h2>
        </div>
        <form>
          <div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Identifikator</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi en formell identifikasjon til tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) =>
                    handleIdentifierChange(e.currentTarget.value)
                  }
                  isValid={isValidIdentifier}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Tittel</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi det offisielle navnet på tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleTitleChange(e.currentTarget.value)}
                  isValid={isValidTitle}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Organisasjonsnummer</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til den offentlige organisasjonen som
                  har ansvaret for tjenesten, uansett om tjenesten tilbys
                  direkte av den aktuelle offentlige organisasjonen eller er
                  satt bort til andre.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) =>
                    handleOrganizationNumberChange(e.currentTarget.value)
                  }
                  isValid={isValidOrganizationNumber}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Organisasjonskode</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til den offentlige organisasjonen som
                  har ansvaret for tjenesten, uansett om tjenesten tilbys
                  direkte av den aktuelle offentlige organisasjonen eller er
                  satt bort til andre.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleOrgCodeChange(e.currentTarget.value)}
                  isValid={isValidOrgCode}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Telefonnummer</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi kontaktpunkt(er) for tjenesten.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) => handlePhoneChange(e.currentTarget.value)}
                  isValid={isValidPhone}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Epost</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi kontaktpunkt(er) for tjenesten.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleEmailChange(e.currentTarget.value)}
                  isValid={isValidEmail}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Beskrivelse</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å oppgi en tekstlig beskrivelse av tjenesten. Denne
                  egenskapen kan gjentas når beskrivelsen finnes i flere språk.{' '}
                </PopoverPanel>
                <TextField
                  onChange={(e) =>
                    handleDescrptionChange(e.currentTarget.value)
                  }
                  isValid={isValidDescription}
                ></TextField>
              </div>
              <div className={classes['float-child']}>
                <label>Hjemmeside</label>
                <PopoverPanel trigger={questionMark}>
                  Hjemmeside til tjenesten.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleHomepageChange(e.currentTarget.value)}
                  isValid={isValidHomepage}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Er del av</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til en annen tjeneste som denne
                  tjenesten er en del av.
                </PopoverPanel>
                <TextField
                  onChange={(e) => handleIsPartOfChange(e.currentTarget.value)}
                  isValid={isValidPartOf}
                ></TextField>
              </div>
            </div>
            <div className={classes['float-container']}>
              <div className={classes['float-child']}>
                <label>Type</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å indikere type tjeneste i henhold til et
                  kontrollert vokabular.
                </PopoverPanel>
                <Select
                  isMulti
                  options={types}
                  theme={(theme) => ({
                    ...theme,
                    borderRadius: 0,
                    colors: {
                      ...theme.colors,
                      neutral20: '#008fd6',
                    },
                  })}
                  onChange={setSelectTypes}
                />
              </div>
              <div className={classes['float-child']}>
                <label>Sektor</label>
                <PopoverPanel trigger={questionMark}>
                  Brukes til å referere til industri/sektor som den aktuelle
                  offentlige tjenesten er relatert til, eller er ment for. En
                  tjeneste kan relateres til flere industrier/sektorer.
                </PopoverPanel>
                <Select
                  isMulti
                  options={sectors}
                  theme={(theme) => ({
                    ...theme,
                    borderRadius: 0,
                    colors: {
                      ...theme.colors,
                      neutral20: '#008fd6',
                    },
                  })}
                  onChange={setSelectSectors}
                />
              </div>
            </div>
            <div className={classes['resource-registry__submit-button']}>
              <Button onClick={submit}>Registrer</Button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ResourceRegistry;
