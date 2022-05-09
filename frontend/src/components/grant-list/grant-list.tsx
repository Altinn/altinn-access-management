import { Grant } from './models';

export interface GrantListProps {
  grants: Grant[];
}

const GrantList = ({ grants }: GrantListProps) => {
  return (
    <table>
      <thead>
        <th>Header 1</th>
      </thead>
      <tbody>
        {grants.map((grant) => (
          <tr>
            <td>{grant.entity.name}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default GrantList;
